using System.Numerics;
using Arch.Core;
using Arch.System;
using Serilog;
using SkiaSharp;
using StateOfHajimi.Core.Components.GlobalComponents;
using StateOfHajimi.Core.Components.MoveComponents;
using StateOfHajimi.Core.Components.PathComponents;
using StateOfHajimi.Core.Components.ProductComponents;
using StateOfHajimi.Core.Components.RenderComponents;
using StateOfHajimi.Core.Components.StateComponents;
using StateOfHajimi.Core.Components.Tags;
using StateOfHajimi.Core.Enums;
using StateOfHajimi.Core.Maths;
using StateOfHajimi.Core.Utils.Extensions;
using StateOfHajimi.Engine.Data;
using StateOfHajimi.Engine.Enums;
using StateOfHajimi.Engine.Rendering.RenderItems;
using StateOfHajimi.Engine.Utils;

namespace StateOfHajimi.Core.Systems.RenderSystem;

public partial class CollectSystem : BufferBaseSystem
{
    private SKRect _bounds;
    private RenderFrame _renderFrame;
    private DebugSettings _settings;
    private readonly RenderContext _context;
    private readonly TileMap _map;
    
    // 线程同步锁：RenderFrame 不是线程安全的，并行写入必须加锁
    private readonly object _syncLock = new();
    
    // 缓存 QueryDescription 以避免重复分配
    private static readonly QueryDescription _entityQuery = new QueryDescription()
        .WithAll<Position, RenderSize, EntityClass, Health, TeamId, AnimationState>()
        .WithNone<Disabled>();

    private static readonly QueryDescription _buildingQuery = new QueryDescription()
        .WithAll<Position, RenderSize, BuildingClass, Health, TeamId, AnimationState>()
        .WithNone<Disabled>();
        
    private static readonly QueryDescription _colliderQuery = new QueryDescription()
        .WithAll<BodyCollider, Position>()
        .WithAny<EntityClass, BuildingClass>()
        .WithNone<Disabled, IsDying>();

    private static readonly QueryDescription _rallyPointQuery = new QueryDescription()
        .WithAll<IsSelected, AutoProduction, Position>()
        .WithNone<Disabled, IsDying>();

    private static readonly QueryDescription _hoverQuery = new QueryDescription()
        .WithAll<IsHovered, RenderSize, BodyCollider, Position>()
        .WithNone<Disabled, IsDying>();
        
    private static readonly QueryDescription _healthBarQuery = new QueryDescription()
        .WithAll<Position, RenderSize, Health>()
        .WithAny<IsSelected, BuildingClass>()
        .WithNone<Disabled, IsDying>();
        
    private static readonly QueryDescription _floatingTextQuery = new QueryDescription()
        .WithAll<Text, Life, Position, RenderSize>()
        .WithNone<Disabled>();

    private static readonly QueryDescription _debugFlowFieldQuery = new QueryDescription()
        .WithAll<IsSelected, FlowAlgorithm>()
        .WithNone<Disabled>();

    private static readonly Dictionary<EntityType, string> _entityKeyCache = new();

    public CollectSystem(World world, RenderContext context, TileMap map) : base(world)
    {
        _context = context;
        _map = map;
        // 确保第一次访问有值
        _renderFrame = _context.GetBackgroundRenderFrame();
        _bounds = _renderFrame.Bounds;
    }

    // 将主要逻辑放在 BeforeUpdate 或 Update 中手动调用
    public override void BeforeUpdate(in float t)
    {
        base.BeforeUpdate(in t);
        
        // 1. 获取上下文
        _renderFrame = _context.GetBackgroundRenderFrame();
        _bounds = _renderFrame.Bounds;
        _settings = World.GetSingleton<DebugSettings>();

        // 2. 并行收集数据
        // 注意：RenderFrame 必须在每帧开始前被 Clear (由 RenderContext 或 Frame 管理)
        
        CollectMapParallel();      // 并行收集地图 (最大性能瓶颈通常在这里)
        CollectEntitiesParallel(); // 并行收集实体
        CollectBuildingsParallel();
        CollectFlowArrows();       // 保持单线程或简单查询
        
        // 其他轻量级或 Debug 收集可以使用普通查询
        CollectCollidersParallel();
        CollectRallyPoints();
        CollectHoverMasks();
        CollectHealthBars();
        CollectFloatingTexts();
    }

    // --- 1. 并行地图收集 (大幅优化) ---
    private void CollectMapParallel()
    {
        var tileSize = _map.TileSize;
        // 扩大一点渲染范围防止边缘闪烁 (+2)
        var startX = (int)Math.Max(0, _bounds.Left / tileSize);
        var startY = (int)Math.Max(0, _bounds.Top / tileSize);
        var endX = (int)Math.Min(_map.Width, _bounds.Right / tileSize + 1);
        var endY = (int)Math.Min(_map.Height, _bounds.Bottom / tileSize + 1);

        var sheet = AssetsManager.GetSheet("Grass", -1);
        var wallSheet = AssetsManager.GetSheet("CrystalCluster", -1);
        
        // 使用 Parallel.For 并行处理每一行 (Y轴)
        Parallel.For(startY, endY, y =>
        {
            // 技巧：为了减少锁的竞争，我们在当前线程暂存这一行所有的指令
            // 只有当这一行处理完后，才一次性锁住 _renderFrame 写入
            // 注意：RenderFrame 需要支持 AddRange 或者我们需要在锁里循环添加
            // 这里为了代码复用，我们在锁里直接调 AddSprite，因为锁每一行比锁每一个块要快得多
            
            // 如果想更极致，可以创建 ThreadLocal 的 List，但这会增加 GC 压力。
            // 折中方案：在锁内操作，但尽量减少锁的次数？不，这里最简单且高效的是：
            // 计算好所有数据，在 lock 块内仅做最快的 List.Add
            
            for (int x = startX; x < endX; x++)
            {
                var tileType = _map.GetTile(x, y);
                var destRect = new SKRect(x * tileSize, y * tileSize, (x + 1.05f) * tileSize, (y + 1.05f) * tileSize);
                
                // 锁住临界区
                lock (_syncLock)
                {
                    if (sheet != null)
                    {
                        _renderFrame.AddSprite(RenderStyle.Sprite, RenderLayer.BedRock, y, sheet.SkiaImage, sheet.GetSkRect(0), destRect);
                    }
                    if (tileType != TileType.Grass && wallSheet != null)
                    {
                        _renderFrame.AddSprite(RenderStyle.Sprite, RenderLayer.Ground, y, wallSheet.SkiaImage, sheet.GetSkRect(0), destRect);
                    }
                }
            }
        });
    }

    // --- 2. 并行实体收集 (Arch ParallelQuery) ---
    private void CollectEntitiesParallel()
    {
        // 使用 World.ParallelQuery 替代 World.Query
        World.ParallelQuery(in _entityQuery, (ref RenderSize renderSize, ref Position position, ref EntityClass entityClass, ref TeamId teamId, ref AnimationState animationState) =>
        {
            // 视锥剔除 (线程安全，只读)
            if (ClipView(_bounds, renderSize, position)) return;

            var halfW = renderSize.Value.X / 2;
            var halfH = renderSize.Value.Y / 2;
            
            // 这一步可能有字典查找，注意 _entityKeyCache 是否线程安全
            // Dictionary 不是线程安全的！我们可能需要 ConcurrentDictionary 或者预先计算 Key
            // 这里为了安全，建议加锁访问 Cache，或者直接 ToString (ToString会有GC)
            string key;
            lock (_syncLock) { key = GetEntityKeyCache(entityClass.Type); }

            var sheet = AssetsManager.GetSheet(key, teamId.Value);
            if (sheet == null) return;

            var src = sheet.GetSkRect(animationState.Offset + animationState.StartFrame);
            var dest = new SKRect(position.Value.X - halfW, position.Value.Y - halfH, position.Value.X + halfW, position.Value.Y + halfH);

            // 写入 RenderFrame (必须加锁)
            lock (_syncLock)
            {
                _renderFrame.AddSprite(RenderStyle.Sprite, RenderLayer.Sprite, position.Value.Y + halfH, sheet.SkiaImage, src, dest);
            }
        });
    }

    private void CollectBuildingsParallel()
    {
        World.ParallelQuery(in _buildingQuery, (ref RenderSize renderSize, ref Position position, ref BuildingClass buildingClass, ref TeamId teamId, ref AnimationState animationState) =>
        {
            if (ClipView(_bounds, renderSize, position)) return;
            
            string key;
            lock (_syncLock) { key = GetEntityKeyCache(buildingClass.Type); }
            
            var sheet = AssetsManager.GetSheet(key, teamId.Value);
            if (sheet == null) return;
            
            var halfW = renderSize.Value.X / 2;
            var halfH = renderSize.Value.Y / 2;
            var src = sheet.GetSkRect(animationState.Offset + animationState.StartFrame);
            var dest = new SKRect(position.Value.X - halfW, position.Value.Y - halfH, position.Value.X + halfW, position.Value.Y + halfH);

            lock (_syncLock)
            {
                _renderFrame.AddSprite(RenderStyle.Sprite, RenderLayer.Sprite, position.Value.Y + halfH, sheet.SkiaImage, src, dest);
            }
        });
    }
    

    
    private void CollectCollidersParallel()
    {
        if (!_settings.ShowColliderBox) return;
        
        World.ParallelQuery(in _colliderQuery, (ref BodyCollider collider, ref Position position) =>
        {
            lock (_syncLock)
            {
                if(ClipView(_bounds, new RenderSize(collider.Size * 2), position)) return;
                if (collider.Type == BodyType.AABB)
                {
                    var left = position.Value.X - collider.Size.X + collider.Offset.X;
                    var right = position.Value.X + collider.Size.X + collider.Offset.X;
                    var top = position.Value.Y - collider.Size.Y + collider.Offset.Y;
                    var bottom = position.Value.Y + collider.Size.Y + collider.Offset.Y;
                    
                    if (left >= _bounds.Left && right <= _bounds.Right && top >= _bounds.Top && bottom <= _bounds.Bottom)
                    {
                        _renderFrame.AddGeometry(RenderStyle.DebugColliderBox, RenderLayer.Debug, top, new SKRect(left, top, right, bottom), GeometryType.Rect);
                    }
                }
                else if (collider.Type == BodyType.Circle)
                {
                    var centerX = position.Value.X + collider.Offset.X;
                    var centerY = position.Value.Y + collider.Offset.Y;
                     _renderFrame.AddGeometry(RenderStyle.DebugColliderCircle, RenderLayer.Debug, centerY, SKRect.Empty, GeometryType.Circle, new Vector2(centerX, centerY), collider.Size with { Y = 0 });
                }
            }
        });
    }
    
    private void CollectRallyPoints()
    {
        World.Query(in _rallyPointQuery, (ref AutoProduction prod, ref Position position) =>
        {
            if (!prod.Rally.IsSet || prod.Rally.Target == Vector2.Zero) return;
            var endX = prod.Rally.Target.X;
            var endY = prod.Rally.Target.Y;
            var sheet = AssetsManager.GetSheet("RallyFlag");
            if (sheet != null)
            {
                _renderFrame.AddSprite(RenderStyle.Sprite, RenderLayer.Ground, endY + 50, sheet.SkiaImage, sheet.GetSkRect(0), new SKRect(endX - 50, endY - 50, endX + 50, endY + 50));
            }
        });
    }

    private void CollectHoverMasks()
    {
        World.Query(in _hoverQuery, (ref Position pos, ref RenderSize size) =>
        {
            var rect = new SKRect(pos.Value.X - size.Value.X / 2, pos.Value.Y - size.Value.Y / 2, pos.Value.X + size.Value.X / 2, pos.Value.Y + size.Value.Y / 2);
            _renderFrame.AddGeometry(RenderStyle.HoverMask, RenderLayer.Effect, pos.Value.Y + size.Value.Y / 2, rect, GeometryType.HoverMask);
        });
    }

    private void CollectHealthBars()
    {
        World.Query(in _healthBarQuery, (ref Position pos, ref RenderSize size, ref Health health) =>
        {
            if (ClipView(_bounds, size, pos)) return;
            var barWidth = size.Value.X * 0.8f;
            var barHeight = size.Value.Y * 0.03f;
            var left = pos.Value.X - barWidth / 2;
            var top = pos.Value.Y - (size.Value.Y / 2) - barHeight;

            _renderFrame.AddGeometry(RenderStyle.HealthBarBackground, RenderLayer.UI, top, new SKRect(left, top, left + barWidth, top + barHeight), GeometryType.Rect, new Vector2(8f, 0));
            
            var healthPct = Math.Clamp((float)health.Current / health.MaxHp, 0f, 1f);
            if (healthPct > 0)
            {
                var style = healthPct switch
                {
                    < 0.25f => RenderStyle.HealthBarLow,
                    >= 0.3f and <= 0.5f => RenderStyle.HealthBarMedium,
                    _ => RenderStyle.HealthBarHigh
                };
                var currentWidth = barWidth * healthPct;
                _renderFrame.AddGeometry(style, RenderLayer.UI, top, new SKRect(left, top, left + currentWidth, top + barHeight), GeometryType.Rect, new Vector2(8f, 0));
            }
        });
    }
    
    private void CollectFloatingTexts()
    {
        World.Query(in _floatingTextQuery, (ref Text text, ref Position pos, ref RenderSize size, ref Life life) =>
        {
            var percentage = (life.LifeTime - life.Age) / life.LifeTime;
            _renderFrame.AddText(RenderStyle.FloatingText, RenderLayer.Effect, pos.Value.Y, text.Content, pos.Value, size.Value with{Y = percentage});
        });
    }
    
    private void CollectFlowArrows()
    {
        if (!_settings.ShowFlowField) return;
        FlowField? activeFlowField = null;
        World.Query(in _debugFlowFieldQuery, (ref FlowAlgorithm flowAlgo) =>
        {
            if (activeFlowField == null && flowAlgo is { IsActive: true, FlowField: not null })
            {
                activeFlowField = flowAlgo.FlowField;
            }
        });
        if (activeFlowField == null) return;
        
        var tileSize = _map.TileSize;
        var startX = (int)Math.Max(0, _bounds.Left / tileSize - 1);
        var startY = (int)Math.Max(0, _bounds.Top / tileSize - 1);
        var endX = (int)Math.Min(_map.Width, _bounds.Right / tileSize + 2);
        var endY = (int)Math.Min(_map.Height, _bounds.Bottom / tileSize + 2);


        for (var y = startY; y < endY; y++)
        {
            for (var x = startX; x < endX; x++)
            {
                var centerX = x * tileSize + tileSize / 2f;
                var centerY = y * tileSize + tileSize / 2f;
                var worldPos = new Vector2(centerX, centerY);
                var dir = activeFlowField.GetFlowDirection(ref worldPos);
                if (dir != Vector2.Zero)
                {
                    _renderFrame.AddGeometry(RenderStyle.DebugFlowArrow, RenderLayer.Ground, worldPos.Y, 
                        new SKRect(centerX - tileSize / 2f, centerY - tileSize / 2f, centerX + tileSize / 2f, centerY + tileSize / 2f), 
                        GeometryType.Arrow, dir, new Vector2(tileSize * 0.6f, 0));
                }
            }
        }
    }


    private static bool ClipView(SKRect contextBounds, RenderSize renderSize, Position position)
    {
        var halfW = renderSize.Value.X / 2;
        var halfH = renderSize.Value.Y / 2;
        return (position.Value.X + halfW < contextBounds.Left || position.Value.X - halfW > contextBounds.Right ||
                position.Value.Y + halfH < contextBounds.Top || position.Value.Y - halfH > contextBounds.Bottom);
    }

    private string GetEntityKeyCache(EntityType entityType)
    {

        if (_entityKeyCache.TryGetValue(entityType, out var key))
        {
            return key;
        }
        _entityKeyCache[entityType] = entityType.ToString();
        return _entityKeyCache[entityType];
    }
}
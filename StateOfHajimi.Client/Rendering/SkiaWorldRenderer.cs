    using System;
    using System.Collections.Generic;
    using System.Numerics;
    using Arch.Core;
    using Avalonia;
    using SkiaSharp;
    using StateOfHajimi.Client.Utils;
    using StateOfHajimi.Core;
    using StateOfHajimi.Core.Components.MoveComponents;
    using StateOfHajimi.Core.Components.StateComponents;
    using StateOfHajimi.Core.Components.Tags;
    using StateOfHajimi.Core.Enums;

    namespace StateOfHajimi.Client.Rendering;

    public class RenderContext
    {
        public SKCanvas Canvas;
        public SKRect Bounds;
    }

    public class SkiaWorldRenderer(GameEngine gameEngine) : IRenderer
    {
        private World _world => gameEngine.GameWorld;
        public bool IsDebug { get; set; } = true;
        private static readonly SKPaint _spritePaint = new ()
        {
            FilterQuality = SKFilterQuality.None,
            IsAntialias = false
        };
        private static readonly SKPaint _groundPaint = new ();

        private static readonly SKPaint _ColliderPaint = new()
        { 
            Color = SKColors.LawnGreen,
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 2,
            IsAntialias = true
        };
        private static readonly SKPaint _healthBarBgPaint = new()
        {
            Color = SKColors.Black.WithAlpha(180),
            Style = SKPaintStyle.Fill
            
        };

        private static readonly SKPaint _healthBarFgPaint = new()
        {
            Color = SKColors.LimeGreen,
            Style = SKPaintStyle.Fill
        };
        private RenderItem[] _renderItems = new RenderItem[200000];
        private int count = 0;
        private static readonly RenderContext _renderContext = new ();
        private static readonly Dictionary<EntityType, string> _entityKeyCache = new();
        private static readonly Dictionary<BuildingType, string> _buildingKeyCache = new();

        private static readonly QueryDescription _renderBodyCollider = new QueryDescription()
            .WithAll<BodyCollider, Position>()
            .WithAny<EntityClass,BuildingClass>()
            .WithNone<Disabled,IsDying>();
        
        private static readonly QueryDescription _renderEntitiesQuery = new QueryDescription()
            .WithAll<Position, Velocity, RenderSize, EntityClass, Health, TeamId, AnimationState>()
            .WithNone<Disabled>();
        
        private static readonly QueryDescription _renderBuildingsQuery = new QueryDescription()
            .WithAll<Position, RenderSize, BuildingClass, TeamId, AnimationState>()
            .WithNone<Disabled>();
            
        private static readonly QueryDescription _renderSelectionQuery = new QueryDescription()
            .WithAll<Position, BodyCollider, IsSelected, RenderSize>()
            .WithNone<Disabled,IsDying>();
        
        private static readonly QueryDescription _buildingHealthQuery = new QueryDescription()
            .WithAll<Position, RenderSize, Health, BuildingClass>()
            .WithNone<Disabled>();
        
        private static readonly QueryDescription _selectedUnitHealthQuery = new QueryDescription()
            .WithAll<Position, RenderSize, Health, EntityClass, IsSelected>() 
            .WithNone<Disabled,IsDying>();
        public void Initialize()
        {
            
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

        private string GetBuildingKeyCache(BuildingType buildingType)
        {
            if (_buildingKeyCache.TryGetValue(buildingType, out var key))
            {
                return key;
            }
            _buildingKeyCache[buildingType] = buildingType.ToString();
            return _buildingKeyCache[buildingType];
        }
        
        public void Render(SKCanvas canvas, Rect bounds, float zoom, Vector2 cameraPos)
        {
            canvas.Clear(SKColor.Empty);
            count = 0;
            SetRenderContext(canvas, bounds, zoom, cameraPos);
            CollectBuildings(_renderContext.Bounds);
            CollectEntities(_renderContext.Bounds);
            Array.Sort(_renderItems,0, count);
            var saveCount = canvas.Save();
            
            // 矩阵预变换 先平移到屏幕中间 ==> 放缩 ==> 移到相机位置   
            // 绘制时直接在世界地图上绘制 canvas会通过变换移动到世界地图指定地点渲染
            canvas.Translate((float)bounds.Width / 2, (float)bounds.Height / 2);
            canvas.Scale(zoom, zoom);
            canvas.Translate(-cameraPos.X, -cameraPos.Y);
            
            RenderMap(_renderContext);
            RenderItems(_renderContext);
            RenderHealthBars(_renderContext);
            if (IsDebug)
            {
                RenderColliderBox(_renderContext);
            }
            canvas.RestoreToCount(saveCount);
            
            RenderScreenUI(_renderContext, bounds); 
            
        }


        private void CollectEntities(SKRect contextBounds)
        {
            _world.Query(in _renderEntitiesQuery, (ref Position position, ref RenderSize renderSize, ref EntityClass entityClass,ref AnimationState animationState) =>
            {
                if (ClipView(contextBounds, renderSize, position)) return;
                var halfW = renderSize.Value.X / 2;
                var halfH = renderSize.Value.Y / 2;
                var key = GetEntityKeyCache(entityClass.Type);
                var sheet = AssetsManager.GetSheet(key);
                var src = sheet.GetSkRect(animationState.Offset + animationState.StartFrame);
                var dest = new SKRect(position.Value.X - halfW, position.Value.Y - halfH, position.Value.X + halfW,  position.Value.Y + halfH);
                if (count >= 200000) return;
                _renderItems[count++] = new RenderItem(position.Value.Y + halfH, sheet.SkiaImage, src, dest, _spritePaint); 
            });
        }

        private void CollectBuildings(SKRect contextBounds)
        {
            _world.Query(in _renderBuildingsQuery, (ref Position position, ref RenderSize renderSize, ref BuildingClass building, ref AnimationState animationState) =>
            {
                if (ClipView(contextBounds, renderSize, position)) return;
                var halfW = renderSize.Value.X / 2;
                var halfH = renderSize.Value.Y / 2;
                var key =  GetBuildingKeyCache(building.Type);
                var sheet = AssetsManager.GetSheet(key);
                var src = sheet.GetSkRect(animationState.Offset + animationState.StartFrame);
                var dest = new SKRect(position.Value.X - halfW, position.Value.Y - halfH, position.Value.X + halfW,  position.Value.Y + halfH);
                if (count >= 200000) return;
                _renderItems[count++] = new RenderItem(position.Value.Y + halfH, sheet.SkiaImage, src, dest, _spritePaint); 
            });
        }
        private static bool ClipView(SKRect contextBounds, RenderSize renderSize, Position position)
        {
            var halfW = renderSize.Value.X / 2;
            var halfH = renderSize.Value.Y / 2;
            return (position.Value.X + halfW < contextBounds.Left || position.Value.X - halfW > contextBounds.Right ||
                    position.Value.Y + halfH < contextBounds.Top || position.Value.Y - halfH > contextBounds.Bottom);
        }


        /// <summary>
        /// 获取渲染上下文
        /// </summary>
        /// <param name="canvas"></param>
        /// <param name="bounds">屏幕边界</param>
        /// <param name="zoom">缩放</param>
        /// <param name="cameraPos">相机位置</param>
        /// <returns></returns>
        private static void SetRenderContext(SKCanvas canvas, Rect bounds, float zoom, Vector2 cameraPos)
        {
            var viewH = bounds.Height / zoom;
            var viewW = bounds.Width / zoom;
            var viewTop = cameraPos.Y - viewH / 2;
            var viewLeft = cameraPos.X - viewW / 2;
            
            // 世界坐标下的渲染范围
            var viewRect = new SKRect
            (
                (float)viewLeft - 200f, 
                (float)viewTop - 200f, 
                (float)(viewLeft + viewW + 200f), 
                (float)(viewTop + viewH + 200f)
            );
            _renderContext.Bounds = viewRect;
            _renderContext.Canvas = canvas;
        }

        private void RenderScreenUI(RenderContext context, Rect bounds)
        {
            
        }

        private void RenderItems(RenderContext context)
        {

            for(var i = 0; i < count; i++)
            {
                context.Canvas.DrawImage(_renderItems[i].image, _renderItems[i].srcRect, _renderItems[i].destRect, _spritePaint);
            }
        }
        
        private void RenderColliderBox(RenderContext context)
        {
            _world.Query(in _renderBodyCollider, (ref Position position, ref BodyCollider collider) =>
            {
                if (collider.Type == BodyType.AABB)
                {
                    var left = position.Value.X - collider.Size.X + collider.Offset.X;
                    var right = position.Value.X + collider.Size.X + collider.Offset.X;
                    var top = position.Value.Y - collider.Size.Y + collider.Offset.Y;
                    var bottom = position.Value.Y + collider.Size.Y + collider.Offset.Y;
                    if (left < context.Bounds.Left || right > context.Bounds.Right ||
                        top < context.Bounds.Top || bottom > context.Bounds.Bottom) return;
                    context.Canvas.DrawRect(new SKRect(left, top, right, bottom), _ColliderPaint);
                }

                if (collider.Type == BodyType.Circle)
                {
                    var centerX = position.Value.X + collider.Offset.X;
                    var centerY = position.Value.Y + collider.Offset.Y;
                    if (context.Bounds.Contains(centerX,centerY))
                    {
                        context.Canvas.DrawCircle(new SKPoint(centerX,centerY), collider.Size.X, _ColliderPaint);
                    }
                }
                
                
            });
        }
        private void RenderHealthBars(RenderContext context)
        {
            _world.Query(in _buildingHealthQuery, (ref Position pos, ref RenderSize size, ref Health health) =>
            {
                if (ClipView(context.Bounds, size, pos)) return;
                DrawSingleHealthBar(context.Canvas, pos.Value, size.Value, health);
            });
            
            _world.Query(in _selectedUnitHealthQuery, (ref Position pos, ref RenderSize size, ref Health health) =>
            {
                if (ClipView(context.Bounds, size, pos)) return;
                DrawSingleHealthBar(context.Canvas, pos.Value, size.Value, health);
            });
        }

        private void DrawSingleHealthBar(SKCanvas canvas, Vector2 pos, Vector2 size, Health health)
        {

            var barWidth = size.X * 0.8f; 
            var barHeight = 20f;           
            var yOffset = 10f;            
            
            var left = pos.X - barWidth / 2;
            var top = pos.Y - (size.Y / 2) - yOffset - barHeight;
            var bgRect = new SKRect(left, top, left + barWidth, top + barHeight);
            canvas.DrawRect(bgRect, _healthBarBgPaint);
            var healthPct = Math.Clamp((float)health.Current / health.MaxHp, 0f, 1f);
            if (healthPct < 0.3f) _healthBarFgPaint.Color = SKColors.Red;
            else if (healthPct < 0.6f) _healthBarFgPaint.Color = SKColors.Orange;
            else _healthBarFgPaint.Color = SKColors.LimeGreen;
            var currentWidth = barWidth * healthPct;
            if (currentWidth > 0)
            {
                var fgRect = new SKRect(left, top, left + currentWidth, top + barHeight);
                canvas.DrawRect(fgRect, _healthBarFgPaint);
            }
        }
        private void RenderMap(RenderContext context)
        {
            var map = gameEngine.CurrentMap;
            var tileSize = map.TileSize;

            var startX = (int)Math.Max(0, context.Bounds.Left / tileSize);
            var startY = (int)Math.Max(0, context.Bounds.Top / tileSize);
            var endX = (int)Math.Min(map.Width, context.Bounds.Right / tileSize + 1);
            var endY = (int)Math.Min(map.Height, context.Bounds.Bottom / tileSize + 1);
            var sheet = AssetsManager.GetSheet("GroundTexture");
            for (int y = startY; y < endY; y++)
            {
                for (int x = startX; x < endX; x++)
                {
                    var tileType = map.GetTile(x, y);
                    var destRect = new SKRect(x * tileSize, y * tileSize, (x + 1) * tileSize, (y + 1) * tileSize);
                    
                    context.Canvas.DrawImage(sheet.SkiaImage,sheet.GetSkRect(0), destRect, _groundPaint);
                }
            }

        }

        public void Dispose()
        {
            _groundPaint.Dispose();
            _spritePaint.Dispose();
        }
        
    }
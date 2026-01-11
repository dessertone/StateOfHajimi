using System;
using System.Numerics;
using Arch.Core;
using Arch.Core.Extensions;
using Avalonia;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using StateOfHajimi.Client.Utils;
using StateOfHajimi.Core;
using StateOfHajimi.Core.Components.MoveComponents;
using StateOfHajimi.Core.Components.ProductComponents;
using StateOfHajimi.Core.Components.StateComponents;
using StateOfHajimi.Core.Components.Tags;
using StateOfHajimi.Core.Enums;

namespace StateOfHajimi.Client.Rendering;

/// <summary>
/// 负责将游戏世界绘制到DrawingContext
/// </summary>
public class WorldRenderer : IRenderer
{
    private readonly GameEngine _engine;
    private readonly GameCamera _camera;

    // 预定义的查询，避免每帧分配
    private readonly QueryDescription _renderUnitsQuery = new QueryDescription()
        .WithAll<Position, Velocity, BodyCollider, EntityClass, TeamId>();
        
    private readonly QueryDescription _renderBuildingsQuery = new QueryDescription()
        .WithAll<Position, BodyCollider, AutoProduction, BuildingClass, TeamId>();
        
    private readonly QueryDescription _renderSelectionQuery = new QueryDescription()
        .WithAll<Position, BodyCollider, IsSelected>();
    
    
    public WorldRenderer(GameEngine engine, GameCamera camera)
    {
        _engine = engine;
        _camera = camera;
    }

    public void Render(DrawingContext context, Rect bounds)
    {
        RenderMap(context, bounds);
        RenderEntities(context, bounds);
    }

    public void RenderMap(DrawingContext context, Rect bounds)
    {
        var map = _engine.CurrentMap;
        if (map == null) return;

        var tileSize = map.TileSize;
        
        var halfViewW = bounds.Width / 2.0;
        var halfViewH = bounds.Height / 2.0;
        var worldLeft = _camera.Position.X - (halfViewW / _camera.Zoom);
        var worldTop = _camera.Position.Y - (halfViewH / _camera.Zoom);

        var startCol = Math.Max(0, (int)Math.Floor(worldLeft / tileSize));
        var startRow = Math.Max(0, (int)Math.Floor(worldTop / tileSize));
        
        var renderCols = (int)Math.Ceiling(bounds.Width / _camera.Zoom / tileSize) + 2;
        var renderRows = (int)Math.Ceiling(bounds.Height / _camera.Zoom / tileSize) + 2;

        var endCol = Math.Min(map.Width, startCol + renderCols);
        var endRow = Math.Min(map.Height, startRow + renderRows);

        for (var y = startRow; y < endRow; y++)
        {
            for (var x = startCol; x < endCol; x++)
            {
                var tileType = map.GetTile(x, y);
                var brush = AssetsManager.GetTileBrush(tileType);
                
                var worldPos = new System.Numerics.Vector2(x * tileSize, y * tileSize);
                var screenPoint = _camera.WorldToScreen(worldPos);
                
                var size = (tileSize * _camera.Zoom) + 0.5;
                var rect = new Rect(screenPoint.X, screenPoint.Y, size, size);
                
                context.FillRectangle(brush, rect);
            }
        }
    }

    public void RenderEntities(DrawingContext context, Rect bounds)
    {
        _engine.GameWorld.Query(in _renderUnitsQuery, 
            (Entity entity, ref Position position, ref BodyCollider b, ref EntityClass tag) =>
            {
                var screenPos = _camera.WorldToScreen(position.Value);
                if (!IsVisible(screenPos, bounds)) return;

                var textureKey = AssetsManager.GetTextureKey(tag.Type);
                var bitmap = AssetsManager.GetTexture(textureKey);
                if (bitmap != null)
                {
                    DrawSpriteCentered(context, bitmap, screenPos, b.Size * 4, _camera.Zoom);

                }
                else
                {
                    context.DrawEllipse(Brushes.White, null, screenPos, b.Size.X * _camera.Zoom, b.Size.X * _camera.Zoom);
                }
            });
        _engine.GameWorld.Query(in _renderBuildingsQuery,
        (ref Position position, ref BodyCollider b,ref TeamId teamId) =>
        {
            var screenPos = _camera.WorldToScreen(position.Value + b.Offset - new Vector2(0,120));
            if (!IsVisible(screenPos, bounds)) return;
            var bitmap = teamId.Value == 0 ? AssetsManager.GetTexture("LightFactory") : AssetsManager.GetTexture("LightFactory-red");
            if (bitmap != null)
            {
                DrawSpriteCentered(context, bitmap, screenPos, new Vector2( b.Size.X * 3.5f, b.Size.Y * 7), _camera.Zoom);
            }
            if (b.Type == BodyType.AABB)
            {
                var worldColliderCenter = position.Value + b.Offset;
                var screenColliderCenter = _camera.WorldToScreen(worldColliderCenter);
                
                var renderWidth = b.Size.X * 2 * _camera.Zoom;
                var renderHeight = b.Size.Y * 2 * _camera.Zoom;
                var rect = new Rect(
                    screenColliderCenter.X - renderWidth / 2,
                    screenColliderCenter.Y - renderHeight / 2,
                    renderWidth,
                    renderHeight
                );
                var pen = new Pen(Brushes.LimeGreen, 2, dashStyle: DashStyle.Dash);
                context.DrawRectangle(null, pen, rect);
            }
            else if (b.Type == BodyType.Circle)
            {
                 var radius = b.Size.X * _camera.Zoom;
                 var pen = new Pen(Brushes.LimeGreen, 2, dashStyle: DashStyle.Dash);
                 context.DrawEllipse(null, pen, screenPos, radius, radius);
            }
        });

        _engine.GameWorld.Query(in _renderSelectionQuery, 
            (ref Position position, ref BodyCollider b) => 
            {
                var screenPos = _camera.WorldToScreen(position.Value);
                if (!IsVisible(screenPos, bounds)) return;
                
                var pen = new Pen(Brushes.LightGreen, 2);
                // 简单的画个圆圈
                var radius = (b.Size.X / 2 + 5) * _camera.Zoom;
                context.DrawEllipse(null, pen, screenPos, radius, radius);
            });
    }

    /// <summary>
    /// 绘制精灵
    /// </summary>
    /// <param name="context"></param>
    /// <param name="bmp"></param>
    /// <param name="screenPos"></param>
    /// <param name="size"></param>
    /// <param name="zoom"></param>
    public void DrawSprite(DrawingContext context, Bitmap bmp, Point screenPos, Vector2 size, float zoom)
    {
        var drawSize = new Size(size.X * zoom, size.Y * zoom);
        var destRect = new Rect(screenPos, drawSize);
        context.DrawImage(bmp, destRect);
    }
    public void DrawSpriteCentered(DrawingContext context, Bitmap bmp, Point screenPos, Vector2 size, float zoom)
    {
        var width = size.X * zoom;
        var height = size.Y * zoom;
        var destRect = new Rect(screenPos.X - width / 2, screenPos.Y - height / 2, width, height);
        context.DrawImage(bmp, destRect);
    }

    public bool IsVisible(Point screenPos, Rect bounds)
    {
        return screenPos.X >= -100 && screenPos.Y >= -100 && 
               screenPos.X <= bounds.Width + 100 && screenPos.Y <= bounds.Height + 100;
    }
}
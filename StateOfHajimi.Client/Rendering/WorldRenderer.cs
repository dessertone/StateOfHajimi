using System;
using System.Numerics;
using Arch.Core;
using Arch.Core.Extensions;
using Avalonia;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Serilog;
using StateOfHajimi.Client.Utils;
using StateOfHajimi.Core;
using StateOfHajimi.Core.Components.MoveComponents;
using StateOfHajimi.Core.Components.ProductComponents;
using StateOfHajimi.Core.Components.StateComponents;
using StateOfHajimi.Core.Components.Tags;
using StateOfHajimi.Core.Enums;
using AnimationState = StateOfHajimi.Core.Components.StateComponents.AnimationState;

namespace StateOfHajimi.Client.Rendering;

public class WorldRenderer : IRenderer
{
    private readonly GameEngine _engine;
    private readonly GameCamera _camera;
    
    private readonly QueryDescription _renderUnitsQuery = new QueryDescription()
        .WithAll<Position, Velocity, BodyCollider, EntityClass, Health, TeamId>();
    
    private readonly QueryDescription _renderBuildingsQuery = new QueryDescription()
        .WithAll<Position, BodyCollider, AutoProduction, BuildingClass, TeamId, AnimationState>();
        
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
    
    // RenderMap 保持不变...
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
                var worldPos = new Vector2(x * tileSize, y * tileSize);
                var screenPoint = _camera.WorldToScreen(worldPos);
                var size = (tileSize * _camera.Zoom) + 0.5;
                var rect = new Rect(screenPoint.X, screenPoint.Y, size, size);
                var sheet = AssetsManager.GetSheet("GroundTexture");
                var sourceRect = sheet.GetTextureRect(0);
                if(sheet != null)
                    context.DrawImage(sheet.Texture, sourceRect, rect);
                if (tileType == TileType.Wall)
                {
                    var s = Random.Shared.Next(0, 2);
                    sheet = AssetsManager.GetSheet("CrystalCluster") ;
                    sourceRect = sheet.GetTextureRect(0);
                    
                    if(sheet != null)
                        context.DrawImage(sheet.Texture, sourceRect, rect);
                }
                
            }
        }
    }

    public void RenderEntities(DrawingContext context, Rect bounds)
    {
        _engine.GameWorld.Query(in _renderBuildingsQuery,
        (ref Position position, ref BodyCollider b, ref TeamId team, ref BuildingClass buildingType, ref AnimationState  animationState) =>
        {
            
            var screenPos = _camera.WorldToScreen(position.Value + b.Offset - new Vector2(0, 120));
            
            if (!IsVisible(screenPos, bounds)) return;
            
            var sheetKey = GetBuildingSheetKey(buildingType.Type, team.Value);
            var sheet = AssetsManager.GetSheet(sheetKey);
            if (sheet != null)
            {
                var visualSize = b.RenderSize; 
                DrawSpriteFrameCentered(context, sheet, animationState.StartFrame + animationState.Offset, screenPos, visualSize, _camera.Zoom);
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
            if (b.Type == BodyType.Circle)
            {
                var worldColliderCenter = position.Value + b.Offset;
                var screenColliderCenter = _camera.WorldToScreen(worldColliderCenter);
                var radius = b.Size.X * _camera.Zoom;
                var pen = new Pen(Brushes.LimeGreen, 2, dashStyle: DashStyle.Dash);
                context.DrawEllipse(null, pen, screenColliderCenter, radius, radius);
            }
        });
        
        _engine.GameWorld.Query(in _renderUnitsQuery, 
            (Entity entity, ref Position position, ref BodyCollider b, ref Health health, ref EntityClass tag) =>
            {
                if (health.IsDead) return;
                var screenPos = _camera.WorldToScreen(position.Value);
                if (!IsVisible(screenPos, bounds)) return;
                context.DrawEllipse(Brushes.White, null, screenPos, b.Size.X * _camera.Zoom, b.Size.X * _camera.Zoom);
            });
    }
    


    /// <summary>
    /// 
    /// </summary>
    public void DrawSpriteFrameCentered(
        DrawingContext context, 
        SpriteSheet sheet, 
        int frameIndex, 
        Point screenCenterPos, 
        Vector2 worldSize, // 世界坐标系下的物体尺寸
        float zoom)
    {
        var sourceRect = sheet.GetTextureRect(frameIndex);
        var drawWidth = worldSize.X * zoom;
        var drawHeight = worldSize.Y * zoom;

        var destRect = new Rect(
            screenCenterPos.X - drawWidth / 2,
            screenCenterPos.Y - drawHeight / 2,
            drawWidth,
            drawHeight
        );
        context.DrawImage(sheet.Texture, sourceRect, destRect);
    }

    /// <summary>
    /// 
    /// </summary>
    private string GetBuildingSheetKey(BuildingType type, int teamId)
    {
        if (type == BuildingType.LightGoodCatFactory)
        {
            return teamId == 1 ? "LightFactory-red" : "LightFactory";
        }
        return "LightFactory"; // Fallback
    }
    
    public bool IsVisible(Point screenPos, Rect bounds)
    {
        return screenPos.X >= -200 && screenPos.Y >= -200 && 
               screenPos.X <= bounds.Width + 200 && screenPos.Y <= bounds.Height + 200;
    }
    public void DrawSpriteCentered(DrawingContext context, Bitmap bmp, Point screenPos, Vector2 size, float zoom)
    {
    }
}
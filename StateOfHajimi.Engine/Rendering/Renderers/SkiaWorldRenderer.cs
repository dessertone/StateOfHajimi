using System.Numerics;
using Avalonia;
using SkiaSharp;
using StateOfHajimi.Engine.Enums;
using StateOfHajimi.Engine.Rendering.RenderItems;

namespace StateOfHajimi.Engine.Rendering.Renderers;

public class SkiaWorldRenderer : IRenderer, IDisposable
{
    public RenderFrame Frame { get; set; } = new();
    
    // --- 样式仓库 ---
    private readonly Dictionary<RenderStyle, SKPaint> _paintCache = new();
    
    // 基础画笔配置（用于绘制Sprite）
    private readonly SKPaint _spritePaint = new() 
    { 
        FilterQuality = SKFilterQuality.None, 
        IsAntialias = false 
    };

    public void Initialize(RenderFrame frame)
    {
        InitializePaints();
    }

    // 1. 在这里集中定义所有的颜色和样式
    private void InitializePaints()
    {
        SKPaint CreatePaint(SKColor color, SKPaintStyle style, float strokeWidth = 1f, bool antiAlias = true)
        {
            return new SKPaint { Color = color, Style = style, StrokeWidth = strokeWidth, IsAntialias = antiAlias };
        }

        // === Debug ===
        _paintCache[RenderStyle.DebugColliderBox] = CreatePaint(SKColors.Crimson, SKPaintStyle.Stroke, 3f);
        _paintCache[RenderStyle.DebugColliderCircle] = CreatePaint(SKColors.Crimson, SKPaintStyle.Stroke, 3f);
        _paintCache[RenderStyle.DebugFlowArrow] = new SKPaint
        {
            Color = SKColors.DarkSlateBlue.WithAlpha(200),
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 2f,
            IsAntialias = true,
            StrokeCap = SKStrokeCap.Round
        };

        _paintCache[RenderStyle.FloatingText] = CreatePaint(SKColors.DarkOrange, SKPaintStyle.Fill);
        // === Health Bar ===
        _paintCache[RenderStyle.HealthBarBackground] = CreatePaint(SKColors.Black.WithAlpha(180), SKPaintStyle.Fill);
        _paintCache[RenderStyle.HealthBarHigh] = CreatePaint(SKColors.LimeGreen, SKPaintStyle.Fill);
        _paintCache[RenderStyle.HealthBarMedium] = CreatePaint(SKColors.Orange, SKPaintStyle.Fill);
        _paintCache[RenderStyle.HealthBarLow] = CreatePaint(SKColors.Red, SKPaintStyle.Fill);

        // === Effects ===
        // 特殊：Hover Mask 需要 Shader，这里先初始化基础属性，绘制时动态设置 Shader 或者预先创建好
        var hoverPaint = new SKPaint { Style = SKPaintStyle.Fill, IsAntialias = true };
        _paintCache[RenderStyle.HoverMask] = hoverPaint;
    }

    public void Render(SKCanvas canvas, Rect bounds, float zoom, Vector2 cameraPos)
    {
        canvas.Clear(SKColor.Empty);
        var saveCount = canvas.Save();

        // 排序
        Frame.SortCommand();

        // 相机变换
        canvas.Translate((float)bounds.Width / 2, (float)bounds.Height / 2);
        canvas.Scale(zoom, zoom);
        canvas.Translate(-cameraPos.X, -cameraPos.Y);


        var commands = Frame.RenderCommands;
        var sprites = Frame.Sprites;
        var geometries = Frame.Geometries;
        var texts = Frame.Texts;

        for (int i = 0; i < commands.Count; i++)
        {
            var cmd = commands[i];
            switch (cmd.Type)
            {
                case RenderType.Sprite: 
                    DrawSprite(canvas, sprites[cmd.Index]); 
                    break;
                case RenderType.Geometry: 
                    DrawGeometry(canvas, geometries[cmd.Index]); 
                    break;
                case RenderType.Text: 
                    DrawText(canvas, texts[cmd.Index]); 
                    break;
            }
        }
        canvas.RestoreToCount(saveCount);
    }
    

    private void DrawSprite(SKCanvas canvas, RenderSprite data)
    {
        canvas.DrawImage(data.Image, data.SrcRect, data.DestRect, _spritePaint);
    }

    private void DrawGeometry(SKCanvas canvas, RenderGeometry data)
    {
        if (!_paintCache.TryGetValue(data.Style, out var paint))
        {
            return; 
        }

        switch (data.Type)
        {
            case GeometryType.Rect:
                // 处理圆角矩形：查看 ExtraParams 是否有值 (例如 RadiusX)
                if (data.ExtraParams != null && data.ExtraParams.Length > 0)
                {
                    float rx = data.ExtraParams[0].X;
                    float ry = data.ExtraParams.Length > 1 ? data.ExtraParams[1].X : rx; // 支持 xy 不同圆角
                    canvas.DrawRoundRect(data.Rect, rx, ry, paint);
                }
                else
                {
                    canvas.DrawRect(data.Rect, paint);
                }
                break;

            case GeometryType.Circle:
                if (data.Rect.IsEmpty && data.ExtraParams.Length > 0)
                {
                    var center = data.ExtraParams[0]; // Center
                    var radius = data.ExtraParams[1].X; // Radius
                    canvas.DrawCircle(center.X, center.Y, radius, paint);
                }
                else 
                {
                     canvas.DrawCircle(data.Rect.Left, data.Rect.Top, data.ExtraParams[0].X, paint);
                }
                break;

            case GeometryType.Arrow:

                if (data.ExtraParams.Length >= 2)
                {
                    var center = new Vector2(data.Rect.Left, data.Rect.Top);
                    var dir = data.ExtraParams[0];
                    var len = data.ExtraParams[1].X;
                    DrawArrow(canvas, center, dir, len, paint);
                }
                break;
             case GeometryType.HoverMask:
                DrawHoverEffect(canvas, data.Rect, data.ExtraParams);
                break;
        }
    }

    private void DrawText(SKCanvas canvas, RenderText data)
    {
        if (!_paintCache.TryGetValue(data.Style, out var paint))
        {
            return; 
        }
        
        var size = data.ExtraParams[0].X;
        var alpha = data.ExtraParams[0].Y;
        canvas.DrawText(data.Content, new SKPoint(data.Position.X, data.Position.Y), paint);
    }

    // --- 专用绘制方法 ---

    private void DrawArrow(SKCanvas canvas, Vector2 center, Vector2 direction, float length, SKPaint paint)
    {
        var halfLen = length * 0.5f;
        var startX = center.X - direction.X * halfLen;
        var startY = center.Y - direction.Y * halfLen;
        var endX = center.X + direction.X * halfLen;
        var endY = center.Y + direction.Y * halfLen;
        
        canvas.DrawLine(startX, startY, endX, endY, paint);

        var wingLen = length * 0.35f;
        const float angleOffset = 0.5f;
        var angle = MathF.Atan2(direction.Y, direction.X);

        var leftAngle = angle + MathF.PI - angleOffset;
        var leftX = endX + MathF.Cos(leftAngle) * wingLen;
        var leftY = endY + MathF.Sin(leftAngle) * wingLen;

        var rightAngle = angle - MathF.PI + angleOffset;
        var rightX = endX + MathF.Cos(rightAngle) * wingLen;
        var rightY = endY + MathF.Sin(rightAngle) * wingLen;

        canvas.DrawLine(endX, endY, leftX, leftY, paint);
        canvas.DrawLine(endX, endY, rightX, rightY, paint);
    }
    
    private void DrawHoverEffect(SKCanvas canvas, SKRect rect, Vector2[] extraParams)
    {
        var paint = _paintCache[RenderStyle.HoverMask];
        var centerX = rect.MidX;
        var centerY = rect.MidY;
        var radius = rect.Width / 2f;
        
        using (var shader = SKShader.CreateRadialGradient(
                   new SKPoint(centerX, centerY),
                   radius,
                   new [] { SKColors.White.WithAlpha(100), SKColors.White.WithAlpha(0) },
                   null,
                   SKShaderTileMode.Clamp))
        {
            paint.Shader = shader;
            canvas.DrawRect(rect, paint);
            paint.Shader = null;
        }
    }

    public void Dispose()
    {
        _spritePaint.Dispose();
        foreach (var paint in _paintCache.Values)
        {
            paint.Dispose();
        }
        _paintCache.Clear();
    }
}
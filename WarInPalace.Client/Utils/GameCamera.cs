
using System;
using System.Numerics;
using Avalonia;

namespace WarInPalace.Client.Utils;

public class GameCamera
{
    /// <summary>
    /// 相机的世界坐标
    /// </summary>
    public Vector2 Position { get; set; } = Vector2.Zero;
    public float Zoom { get; set; } = 1.0f;

    public Size ViewportSize { get; set; }
    private Vector2 ScreenCenter => new ((float)ViewportSize.Width / 2.0f, (float)ViewportSize.Height / 2.0f);

    public Point WorldToScreen(Vector2 worldPos)
    {
        var relativePos = (worldPos - Position) * Zoom;
        return new Point(
            relativePos.X + ScreenCenter.X,
            relativePos.Y + ScreenCenter.Y
        );
    }
    
    public Vector2 ScreenToWorld(Point screenPos)
    {
        var screenVec = new Vector2((float)screenPos.X, (float)screenPos.Y);
        var relativePos = screenVec - ScreenCenter;
        return (relativePos / Zoom) + Position;
    }

    public void ClampToMap(int mapPixelWidth, int mapPixelHeight)
    {

        var halfWidth = (float)(ViewportSize.Width / 2.0 / Zoom);
        var halfHeight = (float)(ViewportSize.Height / 2.0 / Zoom);

        var minX = halfWidth;
        var minY = halfHeight;
        var maxX = mapPixelWidth - halfWidth;
        var maxY = mapPixelHeight - halfHeight;

        if (maxX < minX) 
        {
            Position = Position with { X = mapPixelWidth / 2.0f };
        }
        else
        {
            Position = Position with { X = Math.Clamp(Position.X, minX, maxX) };
        }
        
        if (maxY < minY)
        {
            Position = Position with { Y = mapPixelHeight / 2.0f };
        }
        else
        {
            Position = Position with { Y = Math.Clamp(Position.Y, minY, maxY) };
        }
    }
}
using System.Numerics;
using Avalonia;

namespace StateOfHajimi.Engine.Rendering;

public class GameCamera
{
    private float _zoom = 0.5f;

    /// <summary>
    /// 相机的世界坐标
    /// </summary>
    public Vector2 Position { get; set; } = Vector2.Zero;
    
    private const float MinZoom = 0.6f;
    private const float MaxZoom = 1.4f;
    public float Zoom
    {
        get => _zoom;
        set => _zoom = Math.Clamp(value, MinZoom, MaxZoom);
    }

    public float Speed { get; set; } = 1300.0f;
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


    /// <summary>
    /// 以屏幕上的某个点为中心进行缩放
    /// </summary>
    /// <param name="zoomDelta">缩放增量</param>
    /// <param name="screenFocusPoint">鼠标在屏幕上的位置</param>
    public void ZoomAt(float zoomDelta, Point screenFocusPoint)
    {
        var worldPosBefore = ScreenToWorld(screenFocusPoint);
        Zoom += zoomDelta;
        var screenPosAfter = WorldToScreen(worldPosBefore);
        var screenOffset = new Vector2(
            (float)(screenPosAfter.X - screenFocusPoint.X),
            (float)(screenPosAfter.Y - screenFocusPoint.Y)
        );
        Position += screenOffset / Zoom;
    }
    public void Move(Vector2 delta) => Position += delta * Speed;
    
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

    public void MoveCameraByPixel(Vector2 delta)
    {
        Position += delta;
    }
}
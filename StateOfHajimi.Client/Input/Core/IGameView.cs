using System.Numerics;
using Avalonia;
using Avalonia.Input;

namespace StateOfHajimi.Client.Input.Core;

public enum CursorType
{
    Default,
    Hand,
    Flag,
    Attack
}

public interface IGameView
{
    Rect ViewportSize { get; }
    
    PointerPoint Pointer { get; set; }
    Point MousePosition { get; set; }
    
    (Point, PointerPoint) GetRelativeInfo(PointerEventArgs e);
    
    Vector2 CameraPosition { get; }
    Vector2 ScreenToWorld(Point pos);
    Point WorldToScreen(Vector2 pos);
    
    void SetCursor(CursorType type);
    void MoveCamera(Vector2 delta);
    void MoveCameraByPixel(Vector2 delta);
    bool ContainPoint(Point point);

    void NotifyDrawSelection(Point start, Point end, bool isSelectingArea);

}
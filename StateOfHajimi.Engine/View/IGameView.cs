using System.Numerics;
using Avalonia;
using Avalonia.Input;
using StateOfHajimi.Engine.Enums;

namespace StateOfHajimi.Engine.View;



public interface IGameView
{
    Rect ViewportSize { get; }
    PointerPoint Pointer { get; set; }
    Point MousePosition { get; set; }
    public void ToggleFullScreen();
    (Point, PointerPoint) GetRelativeInfo(PointerEventArgs e);
    Vector2 ScreenToWorld(Point pos);
    void SetCursor(CursorType type);
    void MoveCamera(Vector2 delta);
    void MoveCameraByPixel(Vector2 delta);
    bool ContainPoint(Point point);
    void NotifyDrawSelection(Point start, Point end, bool isSelectingArea);

}
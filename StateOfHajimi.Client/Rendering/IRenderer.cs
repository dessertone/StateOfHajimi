using System.Numerics;
using Avalonia;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using StateOfHajimi.Client.Utils;

namespace StateOfHajimi.Client.Rendering;

public interface IRenderer
{
    void Render(DrawingContext context, Rect bounds);
    void RenderMap(DrawingContext context, Rect bounds);
    void RenderEntities(DrawingContext context, Rect bounds);
    public void DrawSpriteFrameCentered(DrawingContext context, SpriteSheet sheet, int frameIndex, Point screenCenterPos, Vector2 worldSize, float zoom);
    bool IsVisible(Point screenPos, Rect bounds);
}
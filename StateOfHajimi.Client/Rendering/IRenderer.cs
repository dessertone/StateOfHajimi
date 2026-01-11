using Avalonia;
using Avalonia.Media;
using Avalonia.Media.Imaging;

namespace StateOfHajimi.Client.Rendering;

public interface IRenderer
{
    void Render(DrawingContext context, Rect bounds);
    void RenderMap(DrawingContext context, Rect bounds);
    void RenderEntities(DrawingContext context, Rect bounds);
    void DrawSprite(DrawingContext context, Bitmap bmp, Point screenPos, System.Numerics.Vector2 size, float zoom);
    void DrawSpriteCentered(DrawingContext context, Bitmap bmp, Point screenPos, System.Numerics.Vector2 size, float zoom);
    bool IsVisible(Point screenPos, Rect bounds);
}
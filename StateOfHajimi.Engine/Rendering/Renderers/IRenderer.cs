using System.Numerics;
using Avalonia;
using SkiaSharp;
using StateOfHajimi.Engine.Rendering.RenderItems;

namespace StateOfHajimi.Engine.Rendering.Renderers;

public interface IRenderer : IDisposable
{
    void Initialize(RenderFrame frame);
    void Render(SKCanvas canvas, Rect bounds, float zoom, Vector2 cameraPos);
    public RenderFrame Frame { get; set; }
}
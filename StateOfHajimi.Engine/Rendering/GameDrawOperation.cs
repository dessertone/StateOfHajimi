using System.Numerics;
using Avalonia;
using Avalonia.Media;
using Avalonia.Platform;
using Avalonia.Rendering.SceneGraph;
using Avalonia.Skia;
using SkiaSharp;
using StateOfHajimi.Engine.Rendering.Renderers;

namespace StateOfHajimi.Engine.Rendering;

public class GameDrawOperation(IRenderer Renderer, float Zoom, Vector2 CameraPos, Rect bounds)
    : ICustomDrawOperation
{
    public Rect Bounds { get; } = bounds;
    
    public bool Equals(ICustomDrawOperation? other) => false;
    public void Dispose() { }
    public bool HitTest(Point p) => false;


    public void Render(ImmediateDrawingContext context)
    {
        var lease = context.TryGetFeature<ISkiaSharpApiLeaseFeature>();
        if (lease == null) return;
        using var canvasLease  = lease.Lease();
        var canvas = canvasLease.SkCanvas;
        canvas.Save();
        canvas.ClipRect(new SKRect(0, 0, (float)Bounds.Width, (float)Bounds.Height));
        Renderer.Render(canvas, Bounds, Zoom, CameraPos);
        
        canvas.Restore();

    }

 
}
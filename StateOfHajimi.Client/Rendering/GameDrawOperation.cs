using System.Numerics;
using System.Threading;
using Avalonia;
using Avalonia.Media;
using Avalonia.Platform;
using Avalonia.Rendering.SceneGraph;
using Avalonia.Skia;
using SkiaSharp;

namespace StateOfHajimi.Client.Rendering;

public class GameDrawOperation(IRenderer renderer, float zoom, Vector2 cameraPos, Rect bounds)
    : ICustomDrawOperation
{
    public Rect Bounds { get; } = bounds;
    private readonly IRenderer _renderer = renderer;
    private readonly float _zoom = zoom;
    private readonly Vector2 _cameraPos = cameraPos;
    
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
        _renderer.Render(canvas, Bounds, _zoom, _cameraPos);
        
        canvas.Restore();

    }

 
}
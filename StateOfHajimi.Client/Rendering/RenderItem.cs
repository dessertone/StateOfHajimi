using System;
using System.Numerics;
using SkiaSharp;
using StateOfHajimi.Core.Components.MoveComponents;

namespace StateOfHajimi.Client.Rendering;

public record struct RenderItem(float sortZ, SKImage image, SKRect srcRect, SKRect destRect, SKPaint paint): IComparable<RenderItem>
{
    public float SortZ = sortZ;
    public SKImage Image = image;
    public SKRect SrcRect = srcRect;
    public SKRect DestRect = destRect;
    public SKPaint Paint = paint;
    
    public int CompareTo(RenderItem other)
    {
        var res =  SortZ.CompareTo(other.sortZ);
        return res == 0 ? DestRect.Left.CompareTo(other.DestRect.Left) : SortZ.CompareTo(other.SortZ);
    }
}

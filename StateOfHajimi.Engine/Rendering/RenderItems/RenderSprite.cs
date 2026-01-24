using SkiaSharp;
using StateOfHajimi.Engine.Enums;

namespace StateOfHajimi.Engine.Rendering.RenderItems;

public record struct RenderSprite(RenderStyle Style, SKImage Image, SKRect SrcRect, SKRect DestRect);
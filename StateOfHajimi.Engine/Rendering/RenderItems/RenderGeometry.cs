using System.Numerics;
using SkiaSharp;
using StateOfHajimi.Engine.Enums;

namespace StateOfHajimi.Engine.Rendering.RenderItems;

public record struct RenderGeometry(RenderStyle Style, SKRect Rect, GeometryType Type, Vector2[] ExtraParams);
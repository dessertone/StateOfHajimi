using System.Numerics;

namespace StateOfHajimi.Core.Components.RenderComponents;

public struct RenderSize(Vector2 size)
{
    public Vector2 Value = size;
}
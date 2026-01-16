using System.Numerics;

namespace StateOfHajimi.Core.Components.StateComponents;

public struct RenderSize(Vector2 size)
{
    public Vector2 Value = size;
}
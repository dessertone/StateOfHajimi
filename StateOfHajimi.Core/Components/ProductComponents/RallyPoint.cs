using System.Numerics;

namespace StateOfHajimi.Core.Components.ProductComponents;

public struct RallyPoint(Vector2 target, bool isSet)
{
    public Vector2 Target = target;
    public bool IsSet = isSet;
}
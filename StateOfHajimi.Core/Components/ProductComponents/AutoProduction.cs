using System.Numerics;
using StateOfHajimi.Core.Enums;

namespace StateOfHajimi.Core.Components.ProductComponents;

public struct AutoProduction
{
    public UnitType ProductUnitType;
    public float Progress;
    public float Interval;
    public Vector2 Target;
}
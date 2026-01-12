using System.Numerics;
using StateOfHajimi.Core.Enums;

namespace StateOfHajimi.Core.Components.ProductComponents;

public struct AutoProduction
{
    public EntityType ProductEntityType;
    public float Progress;
    public float Interval;
    public RallyPoint Rally;
}
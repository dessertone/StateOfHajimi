using StateOfHajimi.Core.Enums;

namespace StateOfHajimi.Core.Components.RenderComponents;

public struct Facing(Direction direction)
{
    public Direction Value = direction;
}
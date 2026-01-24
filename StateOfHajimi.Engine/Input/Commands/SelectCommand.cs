using System.Numerics;

namespace StateOfHajimi.Engine.Input.Commands;

public record SelectCommand(Vector2 Start, Vector2 End, bool IsAddSelection) : GameCommand
{
    public override string ToString() => nameof(SelectCommand);
}
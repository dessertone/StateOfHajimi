using System.Numerics;

namespace StateOfHajimi.Engine.Input.Commands;

public record SetRallyCommand(Vector2 position) : GameCommand
{
    public override string ToString() => nameof(SetRallyCommand);
}
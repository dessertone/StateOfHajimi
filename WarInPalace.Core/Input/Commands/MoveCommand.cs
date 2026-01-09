using System.Numerics;

namespace WarInPalace.Core.Input;

public record MoveCommand(Vector2 target, bool isAttackMove) : GameCommand
{
    public override string ToString() => nameof(MoveCommand);
}
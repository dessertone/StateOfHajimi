using System.Numerics;

namespace WarInPalace.Core.Input.Commands;

public record NavigateCommand(Vector2 target, bool isAttackMove) : GameCommand
{
    public override string ToString() => nameof(NavigateCommand);
}
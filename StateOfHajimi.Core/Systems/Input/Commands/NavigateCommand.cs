using System.Numerics;

namespace StateOfHajimi.Core.Systems.Input.Commands;

public record NavigateCommand(Vector2 target, bool isAttackMove, bool isSelectedOld) : GameCommand
{
    public override string ToString() => nameof(NavigateCommand);
}
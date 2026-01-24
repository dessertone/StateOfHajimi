namespace StateOfHajimi.Engine.Input.Commands;

public record SwitchDebugCommand(bool IsSwitch) : GameCommand
{
    public override string ToString() => nameof(SwitchDebugCommand);
}
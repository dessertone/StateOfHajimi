namespace WarInPalace.Core.Input.Commands;

public abstract record GameCommand()
{
    public override string ToString() => nameof(GameCommand);
}
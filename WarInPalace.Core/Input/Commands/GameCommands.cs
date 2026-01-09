namespace WarInPalace.Core.Input;

public abstract record GameCommand()
{
    public override string ToString() => nameof(GameCommand);
}
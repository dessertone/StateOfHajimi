using System.Numerics;

namespace WarInPalace.Core.Input.Commands;

public record SelectCommand(Vector2 Start, Vector2 End, bool IsAddSelection) : GameCommand
{
    public override string ToString() => nameof(SelectCommand);
}
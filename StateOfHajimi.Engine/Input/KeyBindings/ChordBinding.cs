using Avalonia.Input;
using StateOfHajimi.Engine.Enums;

namespace StateOfHajimi.Engine.Input.KeyBindings;

public record struct BindingKey(Key key, KeyModifiers modifiers = KeyModifiers.None);

public class ChordBinding(Key holdKey, Key triggerKey, GameAction action)
{
    public Key HoldKey { get; } = holdKey;
    public Key TriggerKey { get; } = triggerKey;
    public GameAction Action { get; } = action;
}
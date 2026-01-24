using Avalonia.Input;
using StateOfHajimi.Engine.Enums;
using StateOfHajimi.Engine.Input.KeyBindings;

namespace StateOfHajimi.Engine.Input.Core;



public static class InputMap
{
    private static readonly Dictionary<BindingKey, GameAction> _keyBindings = new()
    {
        { new(Key.F11), GameAction.ToggleFullscreen },
        { new(Key.Escape), GameAction.OpenGameMenu},
        { new(Key.R), GameAction.SelectRallyPoint },
    };

    private static readonly List<ChordBinding> _chordBindings = 
    [
        new (Key.F3, Key.D, GameAction.ToggleDebug),
    ];
    
    public static GameAction GetAction(Key triggerKey, KeyModifiers modifiers, HashSet<Key> pressedKeys)
    {
        foreach (var chord in _chordBindings)
        {
            if (chord.TriggerKey == triggerKey && pressedKeys.Contains(chord.HoldKey))
            {
                return chord.Action;
            }
        }
        var binding = new BindingKey(triggerKey, modifiers);
        if (_keyBindings.TryGetValue(binding, out var action))
        {
            return action;
        }
        return GameAction.None;
    }
}
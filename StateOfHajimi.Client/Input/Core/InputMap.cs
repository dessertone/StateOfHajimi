using System.Collections.Generic;
using Avalonia.Input;
using StateOfHajimi.Client.Models.Enums;

namespace StateOfHajimi.Client.Input.Core;

public class InputMap
{
    
    private readonly Dictionary<Key, GameAction> _keyBindings = new()
    {
        { Key.F11, GameAction.ToggleFullscreen },
    };

    public GameAction GetAction(Key key)
    {
        return _keyBindings.TryGetValue(key, out var action) ? action : GameAction.None;
    }
}
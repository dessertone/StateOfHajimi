using System.Collections.Generic;
using System.Numerics;
using Arch.LowLevel;
using WarInPalace.Client.Input;

namespace WarInPalace.Core.Input;

public class InputSnapshot
{
    public Vector2 MouseWorldPosition { get; set; }
    public IReadOnlyList<GameCommand>  Commands { get; set; } 
}
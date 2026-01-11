using System.Numerics;
using StateOfHajimi.Core.Systems.Input.Commands;

namespace StateOfHajimi.Core.Systems.Input;

public class InputSnapshot
{
    public Vector2 MouseWorldPosition { get; set; }
    public IReadOnlyList<GameCommand>  Commands { get; set; } 
}
using System.Numerics;
using SkiaSharp;
using StateOfHajimi.Engine.Input.Commands;

namespace StateOfHajimi.Engine.Input;

public class InputSnapshot
{
    public Vector2 MouseWorldPosition { get; set; }
    public IList<GameCommand>  Commands { get; set; } 
}
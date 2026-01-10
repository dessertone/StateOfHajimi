using System.Numerics;

namespace WarInPalace.Core.Components.MoveComponents;

public struct Position
{
    public Vector2 Value;
    
    public Position(float x, float y) => Value = new Vector2(x, y);
    public Position(Vector2 v) => Value = v;
}
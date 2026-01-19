using System.Numerics;
using StateOfHajimi.Core.Enums;

namespace StateOfHajimi.Core.Utils.Extensions;

public static class DirectionExtensions
{
    
    public static Direction Opposite(this Vector2 v)
    {
        if (v == Vector2.Zero) return Direction.South;
        var angle = (float)(Math.Atan2(v.Y, v.X) * 180 / Math.PI) + 112.5f;
        angle %= 360;
        if (angle < 0) angle += 360;
        var index = (int)(angle / 45f) % 8;
        
        return index switch
        {
            0 => Direction.North,
            1 => Direction.NorthEast,
            2 => Direction.East,
            3 => Direction.SouthEast,
            4 => Direction.South,
            5 => Direction.SouthWest,
            6 => Direction.West,
            7 => Direction.NorthWest,
            _ => Direction.South
        };
    }
}
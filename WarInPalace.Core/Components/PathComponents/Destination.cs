using System.Numerics;

namespace WarInPalace.Core.Components.PathComponents;

/// <summary>
/// 对象的移动目的地
/// </summary>
public struct Destination
{ 
    public Vector2 Value;
    public float StopDistanceSquared;
    public bool IsActive;
    public Destination(Vector2 v, float stopDistanceSquared = 1.0f)
    {
        Value = v;
        StopDistanceSquared = stopDistanceSquared;
    }
    public Destination(Vector2 v)
    {
        Value = v;
    }
}
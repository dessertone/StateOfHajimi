using System.Numerics;

namespace WarInPalace.Core.Components;

/// <summary>
/// 对象实际移动的矢量
/// </summary>
public struct Velocity
{
    public Vector2 Value;
    public Velocity(Vector2 v) => Value = v;
    public Velocity(float x, float y) => Value = new Vector2(x, y);
}
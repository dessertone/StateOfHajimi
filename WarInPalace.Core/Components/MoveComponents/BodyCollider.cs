using System.Numerics;
using WarInPalace.Core.Enums;

namespace WarInPalace.Core.Components.MoveComponents;

public struct BodyCollider
{
    public BodyType Type;
    
    public Vector2 Size; 
        
    public Vector2 Offset; 
    public float AvoidanceForce; 
    
    public static BodyCollider CreateCircle(float radius, float force = 1f) => 
        new() { Type = BodyType.Circle, Size = new Vector2(radius, 0), AvoidanceForce = force };
            
    public static BodyCollider CreateBox(float width, float height, float force = 100f) =>
        new() { Type = BodyType.AABB, Size = new Vector2(width / 2, height / 2), AvoidanceForce = force };
}
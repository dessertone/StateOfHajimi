using System.Numerics;
using StateOfHajimi.Core.Enums;

namespace StateOfHajimi.Core.Components.MoveComponents;

public struct BodyCollider(BodyType bodyType, Vector2 size, Vector2 offset, float avoidanceForce)
{
    public BodyType Type = bodyType;
    public Vector2 Size = size;
    public Vector2 Offset = offset; 
    public float AvoidanceForce = avoidanceForce;
}
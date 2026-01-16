using System.Numerics;
using StateOfHajimi.Core.Components.MoveComponents;
using StateOfHajimi.Core.Enums;

namespace StateOfHajimi.Core.Maths;

public struct CollisionResult
{
    public bool HasCollision;
    public Vector2 Normal;    
    public float Penetration; 
}

public static class CollisionResolver
{
    public static CollisionResult CalculateCollision(
        Vector2 posA, BodyCollider colA, 
        Vector2 posB, BodyCollider colB)
    {
        // Size是半长
        Vector2 centerA = posA + colA.Offset;
        Vector2 centerB = posB + colB.Offset;
        
        if (colA.Type == BodyType.Circle && colB.Type == BodyType.Circle)
        {
            return SolveCircleToCircle(centerA, colA.Size.X, centerB, colB.Size.X);
        }
        else if (colA.Type == BodyType.Circle && colB.Type == BodyType.AABB)
        {
            return SolveCircleToAABB(centerA, colA.Size.X, centerB, colB.Size);
        }
        else if (colA.Type == BodyType.AABB && colB.Type == BodyType.Circle)
        {
            var res = SolveCircleToAABB(centerB, colB.Size.X, centerA, colA.Size);
            res.Normal = -res.Normal; 
            return res;
        }
        
        return new CollisionResult { HasCollision = false };
    }

    private static CollisionResult SolveCircleToCircle(Vector2 posA, float radiusA, Vector2 posB, float radiusB)
    {
        var distSq = Vector2.DistanceSquared(posA, posB);
        var minDist = radiusA + radiusB;
        
        if (distSq >= minDist * minDist) 
            return new CollisionResult { HasCollision = false };

        var dist = MathF.Sqrt(distSq);
        var normal = dist > 0.0001f ? (posA - posB) / dist : Vector2.UnitX; 
        
        return new CollisionResult
        {
            HasCollision = true,
            Normal = normal,
            Penetration = minDist - dist
        };
    }

    private static CollisionResult SolveCircleToAABB(Vector2 circlePos, float radius, Vector2 boxCenter, Vector2 boxHalfSize)
    {
        var difference = circlePos - boxCenter;
        var clamped = Vector2.Clamp(difference, -boxHalfSize, boxHalfSize);
        var closest = boxCenter + clamped;
        var direction = circlePos - closest;
        var distSq = direction.LengthSquared();
        
        if (distSq > 0.0001f)
        {
            if (distSq >= radius * radius) 
                return new CollisionResult { HasCollision = false };

            var dist = MathF.Sqrt(distSq);
            return new CollisionResult
            {
                HasCollision = true,
                Normal = direction / dist,
                Penetration = radius - dist
            };
        }
        
        var distX = boxHalfSize.X - MathF.Abs(difference.X);
        var distY = boxHalfSize.Y - MathF.Abs(difference.Y);
        
        if (distX < 0 || distY < 0) 
            return new CollisionResult { HasCollision = true, Normal = Vector2.UnitX, Penetration = radius };
        Vector2 normal;
        float penetration;
        if (distX < distY) 
        {   
            normal = new Vector2(MathF.Sign(difference.X), 0);
            if (normal.X == 0) normal.X = 1; 
            penetration = distX + radius; 
        }
        else
        {
            normal = new Vector2(0, MathF.Sign(difference.Y));
            if (normal.Y == 0) normal.Y = 1;
            penetration = distY + radius;
        }
        return new CollisionResult
        {
            HasCollision = true,
            Normal = normal,
            Penetration = penetration
        };
    }
}
using System;
using System.Numerics;
using WarInPalace.Core.Components;
using WarInPalace.Core.Components.MoveComponents;
using WarInPalace.Core.Enums;

namespace WarInPalace.Core.Utils;

// 定义一个碰撞结果结构体
public struct CollisionResult
{
    public bool HasCollision;
    public Vector2 Normal;    // 碰撞法线（指向“应该被推开”的方向，即从障碍物指向自己）
    public float Penetration; // 穿透深度
}

public static class CollisionResolver
{
    /// <summary>
    /// 计算碰撞详情
    /// </summary>
    public static CollisionResult CalculateCollision(
        Vector2 posA, BodyCollider colA, 
        Vector2 posB, BodyCollider colB)
    {
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
            res.Normal = -res.Normal; // 法线反转
            return res;
        }
        
        return new CollisionResult { HasCollision = false };
    }

    private static CollisionResult SolveCircleToCircle(Vector2 posA, float radiusA, Vector2 posB, float radiusB)
    {
        var distSq = Vector2.DistanceSquared(posA, posB);
        var minDist = radiusA + radiusB;
        
        if (distSq >= minDist * minDist || distSq < 0.0001f) 
            return new CollisionResult { HasCollision = false };

        var dist = MathF.Sqrt(distSq);
        
        Vector2 normal = dist > 0.0001f ? (posA - posB) / dist : Vector2.UnitX; 
        
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
        
        if (distSq >= radius * radius || distSq < 0.0001f) 
            return new CollisionResult { HasCollision = false };

        var dist = MathF.Sqrt(distSq);
        
        Vector2 normal;
        float penetration;
        if (dist > 0.0001f)
        {
            normal = direction / dist;
            penetration = radius - dist;
        }
        else
        {
            normal = Vector2.Normalize(difference); 
            penetration = radius; 
        }
        return new CollisionResult
        {
            HasCollision = true,
            Normal = normal,
            Penetration = penetration
        };
    }
}
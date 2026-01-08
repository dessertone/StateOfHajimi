using System;
using System.Numerics;
using Arch.Core;
using Arch.Core.Extensions;
using WarInPalace.Core.Components;
using WarInPalace.Core.Enums;
using WarInPalace.Core.Utils;

namespace WarInPalace.Core.Systems;

public class AvoidanceSystem : BaseSystem
{
    private static readonly QueryDescription _avoidanceQuery = new QueryDescription()
        .WithAll<BodyCollider, Position, Velocity>();
    
    private static readonly SpatialGrid _spatialGrid = SpatialGrid.Instance; 

    public AvoidanceSystem(World world) : base(world)
    {
    }

    public override void Update(float deltaTime)
    {

        GameWorld.Query(in _avoidanceQuery, (Entity curEntity, ref BodyCollider c, ref Position p, ref Velocity v) =>
        {
            if (c.Type == BodyType.AABB) return;

            Vector2 separationForce = Vector2.Zero;
            int contactCount = 0;

            foreach (var otherEntity in _spatialGrid.Retrieve(p.Value))
            {
                if (curEntity == otherEntity) continue;

                ref var otherPos = ref otherEntity.Get<Position>();
                ref var otherCol = ref otherEntity.Get<BodyCollider>();
                
                var result = CollisionResolver.CalculateCollision(
                    p.Value, c, 
                    otherPos.Value, otherCol
                );

                if (!result.HasCollision) continue;
                // 使用相对速度
                var otherVelVec = Vector2.Zero;
                if (otherEntity.Has<Velocity>()) otherVelVec = otherEntity.Get<Velocity>().Value;
                
                var relativeVelocity = v.Value - otherVelVec;

                // 用投影判断是否在远离
                var velAlongNormal = Vector2.Dot(relativeVelocity, result.Normal);

                // 抵消垂直切线方向的力
                if (velAlongNormal < 0)
                {
                    v.Value -= result.Normal * velAlongNormal;
                }
                
                // 离得越近排斥力越大
                separationForce += result.Normal * (result.Penetration * otherCol.AvoidanceForce);
                    
                contactCount++;
            } 
            if (contactCount > 0)
            {
                v.Value += separationForce * deltaTime;
            }
        });
    }
}
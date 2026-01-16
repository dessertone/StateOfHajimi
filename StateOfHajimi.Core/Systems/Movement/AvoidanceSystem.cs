using System.Numerics;
using Arch.Core;
using Arch.Core.Extensions;
using Arch.System;
using Arch.System.SourceGenerator;
using Serilog;
using StateOfHajimi.Core.Components.MoveComponents;
using StateOfHajimi.Core.Components.Tags;
using StateOfHajimi.Core.Enums;
using StateOfHajimi.Core.Maths;
using StateOfHajimi.Core.Utils;

namespace StateOfHajimi.Core.Systems.Movement;

public partial class AvoidanceSystem : BaseSystem<World,float>
{
    
    private static readonly SpatialGrid _spatialGrid = SpatialGrid.Instance; 
    private const float Epsilon = 0.1f;
    public AvoidanceSystem(World world) : base(world) { }
    
    [Query]
    [All<BodyCollider, Position, Velocity>, None<Disabled,IsDying>]
    public void HandleCollision(Entity curEntity, ref BodyCollider c, ref Position p, ref Velocity v)
    {
        if (c.Type == BodyType.AABB) return;

        var avoidanceNormalSum = Vector2.Zero;
        var contactCount = 0;

        foreach (var otherEntity in _spatialGrid.Retrieve(p.Value))
        {
            if (curEntity == otherEntity) continue;

            if (otherEntity.Has<Position, BodyCollider>())
            {
                ref var otherPos = ref otherEntity.Get<Position>();
                ref var otherCol = ref otherEntity.Get<BodyCollider>();
                var result = CollisionResolver.CalculateCollision(
                    p.Value, c, 
                    otherPos.Value, otherCol
                );
                if (!result.HasCollision) continue;
                if (result.Penetration < Epsilon) continue;
                
                var baseWeight = otherCol.AvoidanceForce;
                var weight = baseWeight * (1.0f + result.Penetration);

                avoidanceNormalSum += result.Normal * weight;
                contactCount++;
            }
            
        } 
        if (contactCount > 0)
        {
            var avgNormal = Vector2.Normalize(avoidanceNormalSum);

            var velocityProjected = Vector2.Dot(v.Value, avgNormal);

            if (velocityProjected < 0)
            {
                v.Value -= avgNormal * velocityProjected;
                // v.Value *= 0.95f; 
            }
        }
    }
}
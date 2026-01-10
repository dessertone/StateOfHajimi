using System.Numerics;
using Arch.Core;
using WarInPalace.Core.Components;
using WarInPalace.Core.Components.PathComponents;

namespace WarInPalace.Core.Systems;

public class NavigationSystem: BaseSystem
{
    private static readonly QueryDescription _navigatonQuery = new QueryDescription().WithAll<Position, Velocity, MoveSpeed, Destination>();
    
    public NavigationSystem(World world) : base(world)
    {
        
    }

    public override void Update(float deltaTime)
    {
        GameWorld.Query(in _navigatonQuery, (ref Position p, ref Velocity v, ref MoveSpeed s, ref Destination dest) =>
        {
            
            if (!dest.IsActive ) 
            {
                return;
            }
            var dir = dest.Value - p.Value;
            var distance = dir.LengthSquared();
            if (distance <= dest.StopDistanceSquared)
            {
                p.Value = dest.Value;
                v.Value = Vector2.Zero;
                dest.IsActive = false;
                return;
            }
            
            // 更新速度方向
            v.Value = Vector2.Normalize(dir) * s.Value;
            
        });
    }
}
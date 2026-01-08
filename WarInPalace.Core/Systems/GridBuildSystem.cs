using Arch.Core;
using WarInPalace.Core.Components;
using WarInPalace.Core.Utils;

namespace WarInPalace.Core.Systems;

public class GridBuildSystem: BaseSystem
{
    
    private static readonly SpatialGrid _spatialGrid = SpatialGrid.Instance; 
    
    private static readonly QueryDescription _buildGridQuery = new QueryDescription()
        .WithAll<BodyCollider, Position, Velocity>();
    
    public GridBuildSystem(World world) : base(world)
    {
        
    }

    public override void Update(float deltaTime)
    {
        _spatialGrid.Clear();
        GameWorld.Query(in _buildGridQuery, (Entity entity, ref Position pos) =>
        {
            _spatialGrid.Add(entity, pos.Value);
        });
        
    }
}
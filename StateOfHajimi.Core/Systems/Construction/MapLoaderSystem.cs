using System.Diagnostics;
using System.Numerics;
using Arch.Core;
using Arch.System;
using Serilog;
using StateOfHajimi.Core.Components.MoveComponents;
using StateOfHajimi.Core.Components.RenderComponents;
using StateOfHajimi.Core.Components.StateComponents;
using StateOfHajimi.Core.Enums;
using StateOfHajimi.Core.Map;
using StateOfHajimi.Core.Maths;
using StateOfHajimi.Core.Utils;

namespace StateOfHajimi.Core.Systems.Construction;

public partial class MapLoaderSystem
{
    
    private static readonly SpatialGrid _spatialGrid = SpatialGrid.Instance;
    private readonly TileMap _tileMap;
    private readonly World _world;
    
    public MapLoaderSystem(World world, TileMap map) 
    {
        _world = world;
        _tileMap = map;
    }
    
    
    public void LoadMap(TileMap map)
    {
        for (int y = 0; y < map.Height; y++)
        {
            for (int x = 0; x < map.Width; x++)
            {
                var type = map.GetTile(x, y);
                
                if (type is TileType.Wall or TileType.Water)
                {
                    CreateStaticObstacle(map, x, y, type);
                }
            }
        }
    }
    
    private void CreateStaticObstacle(TileMap map, int x, int y, TileType type)
    {
        var pos = map.GridToWorldCenter(x, y);
        var position = new Position { Value = pos };
        var entity = _world.Create(
            position,
            new BodyCollider 
            { 
                Type = BodyType.AABB, 
                Size = new Vector2(map.TileSize / 2, map.TileSize / 2),
                AvoidanceForce = 99999f, 
                Offset = Vector2.Zero
            },
            new RenderSize(new Vector2(map.TileSize, map.TileSize))
            // TODO 加一个MapObstacle标签组件方便管理
        );
        _spatialGrid.Add(entity, position.Value); 
    }
}
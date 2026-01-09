using System.Numerics;
using Arch.Core;
using WarInPalace.Core.Components;
using WarInPalace.Core.Enums;
using WarInPalace.Core.Map;
using WarInPalace.Core.Utils;

namespace WarInPalace.Core.Systems;

public class MapLoaderSystem: BaseSystem
{
    
    private static readonly SpatialGrid _spatialGrid = SpatialGrid.Instance;
    
    private readonly TileMap _tileMap;
    
    public MapLoaderSystem(World world, TileMap map) : base(world)
    {
        _tileMap = map;
    }

    public override void Update(float deltaTime)
    {
        LoadMap(_tileMap);
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
        var entity = GameWorld.Create(
            position,
            new BodyCollider 
            { 
                Type = BodyType.AABB, 
                Size = new Vector2(map.TileSize / 2, map.TileSize / 2),
                AvoidanceForce = 99999f, 
                Offset = Vector2.Zero
            }
            // TODO 加一个MapObstacle标签组件方便管理
        );
        _spatialGrid.Add(entity, position.Value); 
    }
}
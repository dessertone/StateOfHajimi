using System.Numerics;
using System.Runtime.InteropServices.Marshalling;
using Arch.Core;
using StateOfHajimi.Core.Components.MoveComponents;
using StateOfHajimi.Core.Components.PathComponents;
using StateOfHajimi.Core.Components.Tags;
using StateOfHajimi.Core.Enums;
using StateOfHajimi.Core.Map;
using StateOfHajimi.Core.Systems;
using StateOfHajimi.Core.Utils;
using StateOfHajimi.Core.Components;
using StateOfHajimi.Core.Components.ProductComponents;
using StateOfHajimi.Core.Components.StateComponents;
using StateOfHajimi.Core.Systems.Construction;
using StateOfHajimi.Core.Systems.Input;
using StateOfHajimi.Core.Systems.Movement;
using StateOfHajimi.Core.Systems.Production;

namespace StateOfHajimi.Core;

public class GameEngine:IDisposable
{
    /// <summary>
    /// 当前 ECS 结构下的世界
    /// </summary>
    public World GameWorld { get; private set; }
    /// <summary>
    /// 系统集群
    /// </summary>
    private SystemGroup _systemGroup = new ();

    /// <summary>
    /// 输入桥接器
    /// </summary>
    public InputBridge Bridge { get; } = new();
    /// <summary>
    /// 网格地图
    /// </summary>
    public TileMap CurrentMap { get; private set; } = new TileMap(100, 100, 40);
    
    /// <summary>
    /// 初始化定义系统更新顺序
    /// </summary>
    public GameEngine()
    {
        
        GameWorld = World.Create();
        GenerateDemoMap(CurrentMap);
        
        // 初始化网格
        _systemGroup.Add(new GridBuildSystem(GameWorld));
        // 初始化地图
        _systemGroup.Add(new MapLoaderSystem(GameWorld, CurrentMap));
        // 批量处理命令
        _systemGroup.Add(new CommandDispatchSystem(GameWorld, Bridge.CurSnapshot));
        // 工厂生产系统
        _systemGroup.Add(new AutoProductSystem(GameWorld));
        // 导航系统
        _systemGroup.Add(new NavigationSystem(GameWorld));
        // 避让系统
        _systemGroup.Add(new AvoidanceSystem(GameWorld));
        // 移动系统
        _systemGroup.Add(new MovementSystem(GameWorld));
        
        _systemGroup.Initialize();
        
        Test();
    }

    private void Test()
    {
        /*var speed = 200f;
        for (int i = 0; i < 10; ++i)
        {
            GameWorld.Create(
                new Position(100 + 40 * i,40),
                new Velocity(0, 0), 
                new MoveSpeed(speed), 
                new Destination{StopDistanceSquared = 1.0f}, 
                BodyCollider.CreateCircle(27, force: speed / 1e4f),
                
                new Selectable());
        }*/
        GameWorld.Create(
            new Position(500, 500), // 位置
            new Velocity(0, 0),
            new AutoProduction{Interval = 5f, ProductUnitType = UnitType.LightweightMechanicalCat, Progress = 0}, // 自动生产
            new isProductionEnabled(), // 是否是生产状态
            new BodyCollider 
            { 
                Type = BodyType.AABB, 
                Size = new Vector2(250, 125),
                AvoidanceForce = 99999f, 
                Offset = new Vector2(0, 250)
            }, // 碰撞体
            new BuildingClass{ Type = BuildingType.LightGoodCatFactory}, // 建筑类型
            new TeamId(0) // 阵营id
        );
        GameWorld.Create(
            new Position(2000, 500), // 位置
            new Velocity(0, 0),
            new AutoProduction{Interval = 5f, ProductUnitType = UnitType.LightweightMechanicalCat, Progress = 0}, // 自动生产
            new isProductionEnabled(), // 是否是生产状态
            new BodyCollider 
            { 
                Type = BodyType.AABB, 
                Size = new Vector2(250, 125),
                AvoidanceForce = 99999f, 
                Offset = new Vector2(0, 250)
            }, // 碰撞体
            new BuildingClass{ Type = BuildingType.LightGoodCatFactory}, // 建筑类型
            new TeamId(1) // 阵营id
        );
    }
    
    /// <summary>   
    /// 游戏更新逻辑
    /// </summary>
    /// <param name="deltaTime"></param>
    public void Update(float deltaTime)
    {
        Bridge.CaptureCurrentShot();
        _systemGroup.Update(deltaTime);
    }

    /// <summary>
    /// 释放资源
    /// </summary>
    public void Dispose()
    {
        GameWorld.Dispose();
    }
    
    /// <summary>
    /// 生成随机地图
    /// </summary>
    /// <param name="map"></param>
    private void GenerateDemoMap(TileMap map)
    {
        var random = new Random();

        for (int y = 0; y < map.Height; y++)
        {
            for (int x = 0; x < map.Width; x++)
            {

                map.SetTile(x, y, TileType.Grass);


                if (x == 0 || x == map.Width - 1 || y == 0 || y == map.Height - 1)
                {
                    map.SetTile(x, y, TileType.Wall);
                }
                
                else if (x > 2 && x < map.Width - 2 && y > 2 && y < map.Height - 2)
                {
                    if (random.NextDouble() < 0.01) 
                    {
                        map.SetTile(x, y, TileType.Wall);
                    }
                    else if (random.NextDouble() < 0.01) 
                    {
                        map.SetTile(x, y, TileType.Dirt);
                    }
                }
            }
        }
        int centerX = map.Width / 2;
        int centerY = map.Height / 2;
        int radius = 5;

        for (int y = centerY - radius; y <= centerY + radius; y++)
        {
            for (int x = centerX - radius; x <= centerX + radius; x++)
            {
                if (Vector2.DistanceSquared(new Vector2(x, y), new Vector2(centerX, centerY)) < radius * radius)
                {
                    if (x > 0 && x < map.Width - 1 && y > 0 && y < map.Height - 1)
                    {
                        map.SetTile(x, y, TileType.Water);
                    }
                }
            }
        }
    }
}
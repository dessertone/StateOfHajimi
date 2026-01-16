using System.Numerics;
using Arch.Core;
using Arch.System;
using StateOfHajimi.Core.Components.MoveComponents;
using StateOfHajimi.Core.Components.Tags;
using StateOfHajimi.Core.Enums;
using StateOfHajimi.Core.Map;
using StateOfHajimi.Core.Components.ProductComponents;
using StateOfHajimi.Core.Components.StateComponents;
using StateOfHajimi.Core.Data;
using StateOfHajimi.Core.Systems.AI;
using StateOfHajimi.Core.Systems.Animation;
using StateOfHajimi.Core.Systems.Combat;
using StateOfHajimi.Core.Systems.Construction;
using StateOfHajimi.Core.Systems.Input;
using StateOfHajimi.Core.Systems.Movement;
using StateOfHajimi.Core.Systems.Production;
using AnimationState = StateOfHajimi.Core.Components.StateComponents.AnimationState;

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
    private readonly Group<float> _systems;
    /// <summary>
    /// 输入桥接器
    /// </summary>
    public InputBridge Bridge { get; } = new();
    /// <summary>
    /// 网格地图
    /// </summary>
    public TileMap CurrentMap { get; private set; } = new TileMap(60, 60, 200);
    
    /// <summary>
    /// 初始化定义系统更新顺序
    /// </summary>
    public GameEngine()
    {
        GameWorld = World.Create();
        GenerateDemoMap(CurrentMap);
        _systems = new("systemGroup",
            // 死亡系统
            new DeathSystem(GameWorld),
            // 初始化网格
            new GridBuildSystem(GameWorld), 
            // 初始化AI
            new AISystem(GameWorld),
            // 初始化地图
            new MapLoaderSystem(GameWorld, CurrentMap),
            // 批量处理命令
            new CommandDispatchSystem(GameWorld, Bridge.CurSnapshot),
            // 动画系统
            new AnimationSystem(GameWorld),
            // 工厂生产系统
            new AutoProductSystem(GameWorld),
            // 避让系统
            new AvoidanceSystem(GameWorld),
            // 移动系统
            new MovementSystem(GameWorld)
            );
        _systems.Initialize();
        Test();
    }

    private void Test()
    {
        var animation = GameConfig.GetBuildingAnimation(BuildingType.LittleHajimiFactory_blue)?.StateAnimations;
        if (animation != null)
            GameWorld.Create(
                new Position(5000, 5000), // 位置
                new Velocity(0, 0),
                new AutoProduction
                {
                    Interval = 5f,
                    ProductEntityType = EntityType.LittleHajimi,
                    Progress = 0,
                    Rally = new RallyPoint
                    {
                        IsSet = true,
                        Target = new Vector2(4600, 6000)
                    }
                }, // 自动生产
                new isProductionEnabled(), // 是否是生产状态
                new AnimationState
                {
                    AnimationKey = nameof(BuildingType.LittleHajimiFactory_blue),
                    StartFrame = animation["Running"].StartFrame,
                    EndFrame = animation["Running"].EndFrame,
                    Offset = 0,
                    FrameTimer = 0,
                    FrameDuration = animation["Running"].FrameDuration,
                    IsActive = true,
                    IsLoop = true
                },
                new RenderSize(new Vector2(600, 600)),
                new BodyCollider
                {
                    Type = BodyType.Circle,
                    Size = new Vector2(150, 0),
                    AvoidanceForce = 999999f,
                    Offset = new Vector2(0, 150)
                }, // 碰撞体
                new BuildingClass { Type = BuildingType.LittleHajimiFactory_blue }, // 建筑类型
                new Health()
                {
                    Current = 1000,
                    MaxHp = 1000
                },
                new TeamId(0) // 阵营id
            );
        GameWorld.Create(
            new Position(6000, 5000), // 位置
            new Velocity(0, 0),
            new AutoProduction{Interval = 5f,
                ProductEntityType = EntityType.LittleHajimi, 
                Progress = 0, 
                Rally = new RallyPoint
                {
                    IsSet = true,
                    Target = new Vector2(6000,6000)
                }
            }, // 自动生产
            new AnimationState{AnimationKey = nameof(BuildingType.LittleHajimiFactory_blue), IsLoop=true,StartFrame = 0, EndFrame = 12, Offset = 0, FrameTimer = 0, FrameDuration = 0.1f, IsActive = true},
            new isProductionEnabled(), // 是否是生产状态
            new RenderSize(new Vector2(600, 600)),
            new BodyCollider 
            { 
                Type = BodyType.Circle, 
                Size = new Vector2(150, 0),
                AvoidanceForce = 999999f, 
                Offset = new Vector2(0, 150)
            }, // 碰撞体
            new BuildingClass{ Type = BuildingType.LittleHajimiFactory_red}, // 建筑类型
            new Health()
            {
                Current = 1000,
                MaxHp = 1000
            },
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
        _systems.BeforeUpdate(in deltaTime);
        _systems.Update(in deltaTime);
        _systems.AfterUpdate(in deltaTime);
    }

    /// <summary>
    /// 释放资源
    /// </summary>
    public void Dispose()
    {
        _systems.Dispose();
        GameWorld.Dispose();
    }
    
    /// <summary>
    /// 生成随机地图
    /// </summary>
    /// <param name="map"></param>
    private void GenerateDemoMap(TileMap map)
    {
        /*var random = new Random();*/

        for (int y = 0; y < map.Height; y++)
        {
            for (int x = 0; x < map.Width; x++)
            {

                map.SetTile(x, y, TileType.Grass);


                if (x == 0 || x == map.Width - 1 || y == 0 || y == map.Height - 1)
                {
                    map.SetTile(x, y, TileType.Wall);
                }
                
                /*else if (x > 2 && x < map.Width - 2 && y > 2 && y < map.Height - 2)
                {
                    if (random.NextDouble() < 0.2) 
                    {
                        map.SetTile(x, y, TileType.Wall);
                    }
                    else if (random.NextDouble() < 0.01) 
                    {
                        map.SetTile(x, y, TileType.Dirt);
                    }
                }*/
            }
        }
        int centerX = map.Width / 2;
        int centerY = map.Height / 2;
        int radius = 5;

        /*for (int y = centerY - radius; y <= centerY + radius; y++)
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
        }*/
    }
}
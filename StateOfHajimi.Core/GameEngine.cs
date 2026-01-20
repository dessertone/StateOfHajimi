using System.Diagnostics;
using System.Numerics;
using Arch.Buffer;
using Arch.Core;
using Arch.System;
using Serilog;
using StateOfHajimi.Core.Components.MoveComponents;
using StateOfHajimi.Core.Components.Tags;
using StateOfHajimi.Core.Enums;
using StateOfHajimi.Core.Map;
using StateOfHajimi.Core.Components.ProductComponents;
using StateOfHajimi.Core.Components.StateComponents;
using StateOfHajimi.Core.Data;
using StateOfHajimi.Core.Data.Builders.Bases;
using StateOfHajimi.Core.Navigation;
using StateOfHajimi.Core.Systems.AI;
using StateOfHajimi.Core.Systems.Animation;
using StateOfHajimi.Core.Systems.Combat;
using StateOfHajimi.Core.Systems.Construction;
using StateOfHajimi.Core.Systems.Input;
using StateOfHajimi.Core.Systems.Movement;
using StateOfHajimi.Core.Systems.Production;
using AnimationState = StateOfHajimi.Core.Components.RenderComponents.AnimationState;

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
    public TileMap CurrentMap { get; private set; } = new (200, 200, 50);
    
    /// <summary>
    /// 初始化定义系统更新顺序
    /// </summary>
    public GameEngine()
    {
        GameWorld = World.Create();
        GenerateDemoMap(CurrentMap);
        new MapLoaderSystem(GameWorld, CurrentMap).LoadMap(CurrentMap);
        FlowFieldManager.Instance.Initialize(CurrentMap);
        _systems = new("systemGroup",
            // 死亡系统
            new DeathSystem(GameWorld),
            // 初始化网格
            new GridBuildSystem(GameWorld), 
            // 鼠标检测
            new CursorDetectSystem(GameWorld, Bridge),
            // 初始化AI
            new AISystem(GameWorld),
            // 批量处理命令
            new CommandDispatchSystem(GameWorld, Bridge.CurSnapshot),
            // 工厂生产系统
            new AutoProductSystem(GameWorld),
            // 动画系统
            new AnimationSystem(GameWorld),
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
        var buffer = new CommandBuffer();
        var context = new BuildContext(new Vector2(3000, 6400), 0, new RallyPoint(new Vector2(3000, 8000), true));
        UnitFactory.CreateEntity(buffer, EntityType.LittleHajimiFactory,ref context);
        context.Position.X = 5000;
        context.TeamId = 1;
        context.ExtraData = new RallyPoint(new Vector2(5000, 8000), true);
        UnitFactory.CreateEntity(buffer, EntityType.LittleHajimiFactory,ref context);
        buffer.Playback(GameWorld);
    }
    
    /// <summary>   
    /// 游戏更新逻辑
    /// </summary>
    /// <param name="deltaTime"></param>
    public void Update(float deltaTime)
    {
        Bridge.CaptureCurrentShot();
        _systems.BeforeUpdate(in deltaTime);
        var sw = Stopwatch.StartNew();
        _systems.Update(in deltaTime);
        var t2 = sw.Elapsed.TotalMilliseconds;
        if(t2 > 16) 
            Log.Warning($"LAG DETECTED! LOGIC TOOK: {t2} ms");
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
        int w = map.Width;
        int h = map.Height;

        // 1. 先全部铺满草地
        for (int x = 0; x < w; x++)
        {
            for (int y = 0; y < h; y++)
            {
                map.SetTile(x, y, TileType.Grass);
            }
        }

        // 辅助函数：绘制矩形块
        void DrawRect(int x, int y, int rw, int rh, TileType type)
        {
            for (int i = x; i < x + rw; i++)
            {
                for (int j = y; j < y + rh; j++)
                {
                    map.SetTile(i, j, type);
                }
            }
        }

        // 2. 四周围墙 (防止掉出世界)
        DrawRect(0, 0, w, 1, TileType.Wall);          // 上
        DrawRect(0, h - 1, w, 1, TileType.Wall);      // 下
        DrawRect(0, 0, 1, h, TileType.Wall);          // 左
        DrawRect(w - 1, 0, 1, h, TileType.Wall);      // 右

        // ==========================================
        // 场景 A：中间的“护城河”与“独木桥” (测试瓶颈通过能力)
        // ==========================================
        int midY = h / 2;
        // 左边的水域
        DrawRect(1, midY - 2, (w / 2) - 5, 5, TileType.Water); 
        // 右边的水域
        DrawRect((w / 2) + 5, midY - 2, (w / 2) - 6, 5, TileType.Water);
        // 中间留出的 10 格宽度的桥梁，用于测试大量单位挤过狭窄通道

        // ==========================================
        // 场景 B：左下角的“U型陷阱” (测试局部极小值逃逸)
        // ==========================================
        // 一个开口向上的 U 型墙
        int trapX = 15;
        int trapY = 15;
        DrawRect(trapX, trapY, 20, 2, TileType.Wall);      // 底部
        DrawRect(trapX, trapY, 2, 20, TileType.Wall);      // 左壁
        DrawRect(trapX + 18, trapY, 2, 20, TileType.Wall); // 右壁
        // 如果你点进这个 U 型里面，单位应该能走进去
        // 如果你在 U 型外点对面，单位应该懂得绕过这堵墙，而不是卡在底部

        // ==========================================
        // 场景 C：右上角的“稀疏森林” (测试局部避让与微操)
        // ==========================================
        // 在右上区域放置间隔的树木
        for (int i = w - 40; i < w - 5; i += 4)
        {
            for (int j = 5; j < 35; j += 4)
            {
                // 制造一点错位感，不要完全整齐
                int offset = (j % 8 == 0) ? 2 : 0;
                map.SetTile(i + offset, j, TileType.Tree);
            }
        }

        // ==========================================
        // 场景 D：右下角的“斜向墙壁” (测试锯齿移动)
        // ==========================================
        // 很多网格算法处理斜线很生硬，用来测试流场的平滑度
        int startX = w - 30;
        int startY = h - 30;
        for (int i = 0; i < 20; i++)
        {
            map.SetTile(startX + i, startY + i, TileType.Wall);
            map.SetTile(startX + i + 1, startY + i, TileType.Wall); // 加厚一点
        }
        
        // ==========================================
        // 场景 E：左上角的“房间” (测试室内寻路)
        // ==========================================
        DrawRect(5, h - 30, 20, 20, TileType.Wall); // 房间外壳
        DrawRect(6, h - 29, 18, 18, TileType.Grass); // 挖空内部
        map.SetTile(15, h - 30, TileType.Grass);     // 上方开门
        map.SetTile(16, h - 30, TileType.Grass);
        map.SetTile(15, h - 11, TileType.Grass);     // 下方开门
        map.SetTile(16, h - 11, TileType.Grass);
    }
}
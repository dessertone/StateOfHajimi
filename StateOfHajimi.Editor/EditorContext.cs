using System;
using Arch.Core;
using Arch.System;
using Schedulers;
using StateOfHajimi.Core.Components.GlobalComponents;
using StateOfHajimi.Core.Contexts.Game;
using StateOfHajimi.Core.Systems.Input;
using StateOfHajimi.Core.Systems.RenderSystem;
using StateOfHajimi.Core.Utils.Extensions;
using StateOfHajimi.Engine.Data;
using StateOfHajimi.Engine.Enums;
using StateOfHajimi.Engine.Input;
using StateOfHajimi.Engine.Rendering.RenderItems;
using StateOfHajimi.Engine.View;

namespace StateOfHajimi.Editor;

public class EditorContext : IDisposable, IGameContext
{
    public void Dispose()
    {
        GameWorld.Dispose();
    }

    public TileMap Map { get; set; } = new (100, 100, 50);
    public RenderContext RenderContext { get; set; } = new();
    public World GameWorld { get; init; }
    public IBridge Bridge { get; init; } = new GameBridge();

    private readonly Group<float> _editorSystems;
    public EditorContext()
    {
        GameWorld = World.Create();
        GameWorld.SetSingleton(new DebugSettings());
        World.SharedJobScheduler = new JobScheduler(
            new JobScheduler.Config
            {
                ThreadPrefixName = "ArchJob",
                ThreadCount = Environment.ProcessorCount
            }
        );
        GenerateDemoMap(Map);
        _editorSystems = new Group<float>(
            "EditorSystem",
            [
                new CollectSystem(GameWorld, RenderContext, Map),
                new CommandDispatchSystem(GameWorld,  Bridge.CurSnapshot),
                new CursorDetectSystem(GameWorld,  Bridge),
            ]);
    }
    public void Update(float deltaTime)
    {
        _editorSystems.BeforeUpdate(deltaTime);
        _editorSystems.Update(deltaTime);
        _editorSystems.AfterUpdate(deltaTime);
    }
    
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
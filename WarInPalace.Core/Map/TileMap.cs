using System.Numerics;
using WarInPalace.Core.Enums;

namespace WarInPalace.Core.Map;

public class TileMap
{
    public int Width { get;}
    public int Height { get;}
    public int TileSize { get; }

    // 将 _tiles 改为 public 也是一种选择，但保持封装更好
    private readonly TileType[] _tiles;

    public TileMap(int width, int height, int tileSize)
    {
        Width = width;
        Height = height;
        TileSize = tileSize;
        _tiles = new TileType[width * height];
    }
    
    // === 核心辅助 ===
    private bool IsValid(int x, int y) => x >= 0 && x < Width && y >= 0 && y < Height;

    public void SetTile(int x, int y, TileType tile)
    {
        if (IsValid(x, y))
        {
            _tiles[y * Width + x] = tile;
        }
    }

    public TileType GetTile(int x, int y)
    {
        if(IsValid(x, y)) return _tiles[y * Width + x];
        return TileType.Wall; // 越界默认视为墙壁（不可走）
    }
    
    // === 坐标转换 ===
    
    public (int x, int y) WorldToGrid(Vector2 worldPos)
    {
        // 使用 Floor 确保负坐标处理正确（虽然地图通常从0开始）
        return ((int)(worldPos.X / TileSize), (int)(worldPos.Y / TileSize));
    }
    
    public Vector2 GridToWorldCenter(int x, int y)
    {
        // 返回格子的中心点，这对寻路和单位对齐很重要
        return new Vector2(
            x * TileSize + TileSize * 0.5f, 
            y * TileSize + TileSize * 0.5f
        );
    }

    // === 可行走性判断 ===

    /// <summary>
    /// 判断指定网格坐标是否可以通行
    /// </summary>
    public bool IsWalkable(int x, int y)
    {
        // 1. 越界检查：超出地图范围视为不可走
        if (!IsValid(x, y)) return false;

        // 2. 获取地形类型
        var type = _tiles[y * Width + x];

        // 3. 根据类型判断
        // 你可以根据实际的游戏设计调整这里的逻辑
        return type switch
        {
            TileType.Grass => true,
            TileType.Dirt => true,
            TileType.Water => false, // 假设陆军不能下水
            TileType.Wall => false,
            _ => false
        };
    }

    /// <summary>
    /// 判断指定世界坐标所在的格子是否可以通行
    /// </summary>
    public bool IsWalkable(Vector2 worldPos)
    {
        var (x, y) = WorldToGrid(worldPos);
        return IsWalkable(x, y);
    }
}
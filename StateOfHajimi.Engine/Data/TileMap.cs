using System.Numerics;
using StateOfHajimi.Engine.Enums;

namespace StateOfHajimi.Engine.Data;

public class TileMap
{
    public int Width { get;}
    public int Height { get;}
    public int TileSize { get; }
    
    private readonly TileType[] _tiles;

    public TileMap(int width, int height, int tileSize)
    {
        Width = width;
        Height = height;
        TileSize = tileSize;
        _tiles = new TileType[width * height];
    }
    
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
        return TileType.Wall; 
    }
    
    
    public (int x, int y) WorldToGrid(Vector2 worldPos)
    {
        return ((int)(worldPos.X / TileSize), (int)(worldPos.Y / TileSize));
    }
    
    public Vector2 GridToWorldCenter(int x, int y)
    {
        return new Vector2(
            x * TileSize + TileSize * 0.5f, 
            y * TileSize + TileSize * 0.5f
        );
    }
    

    /// <summary>
    /// 判断指定网格坐标是否可以通行
    /// </summary>
    public bool IsWalkable(int x, int y)
    {
        if (!IsValid(x, y)) return false;
        var type = _tiles[y * Width + x];
        return type switch
        {
            TileType.Grass => true,
            TileType.Tree => false,
            TileType.Water => false, 
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
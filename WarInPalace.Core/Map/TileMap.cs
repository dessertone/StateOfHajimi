using System.Numerics;
using WarInPalace.Core.Enums;

namespace WarInPalace.Core.Map;

public class TileMap
{
    public int Width { get;}
    public int Height { get;}
    public float TileSize { get; }

    public readonly TileType[] _tiles;

    private bool IsValid(int x, int y) => x >= 0 && x < Width && y >= 0 && y < Height;
    public TileMap(int width, int height, float tileSize)
    {
        Width = width;
        Height = height;
        TileSize = tileSize;
        _tiles = new TileType[width * height];
    }

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
        return new Vector2(x * TileSize + TileSize / 2, y * TileSize + TileSize / 2);
    }
}
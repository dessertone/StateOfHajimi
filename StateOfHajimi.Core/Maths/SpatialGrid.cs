using System.Numerics;
using Arch.Core;

namespace StateOfHajimi.Core.Maths;

public class SpatialGrid
{
    /// <summary>
    /// 存放每个网格单元内的实体
    /// </summary>
    private readonly Dictionary<(int, int), List<Entity>> _buckets = new();
    /// <summary>
    /// 每个网格单元大小
    /// </summary>
    private readonly float _cellSize = 500f;

    private static readonly Lazy<SpatialGrid> _lazy = new(()=>new SpatialGrid());
    
    public static SpatialGrid Instance { get; } = _lazy.Value;
    
    private SpatialGrid()
    {
    }
    
    /// <summary>
    /// 通过<see cref="Vector2"/>得到网格单元索引
    /// </summary>
    /// <param name="pos">实体位置</param>
    /// <returns></returns>
    private (int, int) GetCellKey(Vector2 pos) => ((int)(pos.X / _cellSize), (int)(pos.Y / _cellSize));
    
    /// <summary>
    /// 将实体添加进网格中
    /// </summary>
    /// <param name="entity">需要添加的实体</param>
    public void Add(Entity entity, Vector2 pos)
    {
        var key = GetCellKey(pos);
        if (!_buckets.ContainsKey(key))
        {
            _buckets.Add(key, new List<Entity>());
        }
        _buckets[key].Add(entity);
    }

    /// <summary>
    /// 返回指定位置在指定搜索半径下搜索到的所有实体
    /// </summary>
    /// <param name="pos"></param>
    /// <param name="range"></param>
    /// <returns></returns>
    public IEnumerable<Entity> Retrieve(Vector2 pos, float range = 2000)
    {
        var (r, c) = GetCellKey(pos);
        var size = (int)(range * 2 / _cellSize + 2);
        var start = 0 - size / 2;
        for (var col = start; col < start + size; ++col)
        {
            for (var row = start; row < start + size; ++row)
            {
                if (_buckets.TryGetValue((r + row, c + col), out var bucket))
                {
                    foreach (var entity in bucket)
                        yield return entity;
                }
            }
        }
    }
    /// <summary>
    /// 获取指定矩形区域内的所有潜在实体
    /// </summary>
    public IEnumerable<Entity> QueryRect(Vector2 startPos, Vector2 endPos)
    {

        var minX = Math.Min(startPos.X, endPos.X);
        var minY = Math.Min(startPos.Y, endPos.Y);
        var maxX = Math.Max(startPos.X, endPos.X);
        var maxY = Math.Max(startPos.Y, endPos.Y);
        
        var startCol = (int)(minX / _cellSize);
        var startRow = (int)(minY / _cellSize);
        var endCol = (int)(maxX / _cellSize);
        var endRow = (int)(maxY / _cellSize);
        
        for (var col = startCol; col <= endCol; col++)
        {
            for (var row = startRow; row <= endRow; row++)
            {
                if (_buckets.TryGetValue((col, row), out var bucket))
                {
                    foreach (var entity in bucket)
                    {
                        yield return entity;
                    }
                }
            }
        }
    }
    /// <summary>
    /// 清除网格
    /// </summary>
    public void Clear()
    {
        foreach (var list in _buckets.Values)
        {
            list.Clear();
        }
    }
}
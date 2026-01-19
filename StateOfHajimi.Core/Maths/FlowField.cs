using System.Numerics;
using StateOfHajimi.Core.Enums;
using StateOfHajimi.Core.Map;

namespace StateOfHajimi.Core.Maths;

public class FlowField
{
    private int _width;
    private int _height;
    private int _tileSize;
    private byte[] _costField;
    private int[] _integrationField;
    private Vector2[] _vectorField;

    public FlowField(TileMap map)
    {
        _width = map.Width;
        _height = map.Height;
        _tileSize = map.TileSize;
        
        int length = _width * _height;
        _costField = new byte[length];
        _integrationField = new int[length];
        _vectorField = new Vector2[length];

        for (var y = 0; y < _height; y++)
        {
            for (var x = 0; x < _width; x++)
            {
                int index = y * _width + x;
                var type = map.GetTile(x, y);
                _costField[index] = type != TileType.Grass ? (byte)255 : (byte)1;
            }
        }
    }

    /// <summary>
    /// 生成流场
    /// </summary>
    /// <param name="target">世界坐标目标点</param>
    public void Generate(Vector2 target)
    {

        Array.Fill(_integrationField, int.MaxValue);

        var targetX = (int)(target.X / _tileSize);
        var targetY = (int)(target.Y / _tileSize);
        
        if (targetX < 0 || targetX >= _width || targetY < 0 || targetY >= _height) return;
        var openList = new Queue<(int x, int y)>();
        
        var targetIndex = targetY * _width + targetX;
        _integrationField[targetIndex] = 0;
        openList.Enqueue((targetX, targetY));

        while (openList.Count > 0)
        {
            var (cx, cy) = openList.Dequeue();
            int currentIndex = cy * _width + cx;
            var curDist = _integrationField[currentIndex];

            foreach (var neighbor in GetNeighbors(cx, cy))
            {
                var neighborIndex = neighbor.y * _width + neighbor.x;
                var cost = _costField[neighborIndex];
                if (cost == 255) continue;
                if (_integrationField[neighborIndex] > curDist + cost)
                {
                    _integrationField[neighborIndex] = curDist + cost;
                    openList.Enqueue(neighbor);
                }
            }
        }
        
        for (var x = 0; x < _width; x++)
        {
            for (var y = 0; y < _height; y++)
            {
                var index = y * _width + x;
                
                if (_costField[index] == 255 || _integrationField[index] == int.MaxValue) 
                {
                    _vectorField[index] = Vector2.Zero;
                    continue;
                }

                var bestDist = _integrationField[index];
                var bestNeighbor = (x, y); 
                
                foreach (var n in GetNeighbors(x, y))
                {
                    int nIndex = n.y * _width + n.x;
                    if (_integrationField[nIndex] < bestDist)
                    {
                        bestDist = _integrationField[nIndex];
                        bestNeighbor = n;
                    }
                }
                if (bestNeighbor.x == x && bestNeighbor.y == y)
                {
                    _vectorField[index] = Vector2.Zero;
                }
                else
                {
                    Vector2 direction = new Vector2(bestNeighbor.x - x, bestNeighbor.y - y);
                    _vectorField[index] = Vector2.Normalize(direction);
                }
            }
        }
    }
    
    public Vector2 GetFlowDirection(ref Vector2 worldPos)
    {
        var x = (int)(worldPos.X / _tileSize);
        var y = (int)(worldPos.Y / _tileSize);

        if (x < 0 || x >= _width || y < 0 || y >= _height) return Vector2.Zero;
    
        var index = y * _width + x;
        var dir = _vectorField[index];

        if (dir == Vector2.Zero)
        {
            return GetEscapeDirection(x, y);
        }
    
        return dir;
    }
    private Vector2 GetEscapeDirection(int cx, int cy)
    {

        var bestDist = int.MaxValue;
        (int x, int y) bestNeighbor = (-1, 
            -1);

        foreach (var n in GetNeighbors(cx, cy))
        {
            var idx = n.y * _width + n.x;

            if (_costField[idx] != 255 && _integrationField[idx] < bestDist)
            {
                bestDist = _integrationField[idx];
                bestNeighbor = n;
            }
        }
        
        if (bestNeighbor.x != -1)
        {
            return Vector2.Normalize(new Vector2(bestNeighbor.x - cx, bestNeighbor.y - cy));
        }

        return Vector2.Zero;
    }

    private IEnumerable<(int x, int y)> GetNeighbors(int cx, int cy)
    {
        // 十字邻居
        if (cx > 0) yield return (cx - 1, cy);
        if (cx < _width - 1) yield return (cx + 1, cy);
        if (cy > 0) yield return (cx, cy - 1);
        if (cy < _height - 1) yield return (cx, cy + 1);

        // 对角邻居
        if (cx > 0 && cy > 0) yield return (cx - 1, cy - 1);
        if (cx > 0 && cy < _height - 1) yield return (cx - 1, cy + 1);
        if (cx < _width - 1 && cy < _height - 1) yield return (cx + 1, cy + 1);
        if (cx < _width - 1 && cy > 0) yield return (cx + 1, cy - 1);
    }
}
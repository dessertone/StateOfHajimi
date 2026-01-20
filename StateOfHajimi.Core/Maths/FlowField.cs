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

    // 为了避免浮点数计算，使用整数倍率：直线=10，斜角=14
    private const int CostStraight = 10;
    private const int CostDiagonal = 14;

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

        // --- 阶段 1: 生成积分场 (Dijkstra) ---
        while (openList.Count > 0)
        {
            var (cx, cy) = openList.Dequeue();
            int currentIndex = cy * _width + cx;
            var curDist = _integrationField[currentIndex];

            foreach (var neighbor in GetNeighbors(cx, cy))
            {
                var neighborIndex = neighbor.y * _width + neighbor.x;
                
                // 1. 基础墙壁检查
                if (_costField[neighborIndex] == 255) continue;

                // 2. 切角检查 (Corner Cutting Check)
                // 如果是斜向移动，且相邻的两个直线格子中有任意一个是墙，则不允许通行
                if (!CanMoveDiagonal(cx, cy, neighbor.x, neighbor.y)) continue;

                // 3. 计算代价：判断是直线还是斜线
                int moveCost = (cx == neighbor.x || cy == neighbor.y) ? CostStraight : CostDiagonal;
                
                // 地形基础代价 (costField中的值，比如沼泽可以是5)
                int totalCost = moveCost * _costField[neighborIndex];

                if (_integrationField[neighborIndex] > curDist + totalCost)
                {
                    _integrationField[neighborIndex] = curDist + totalCost;
                    openList.Enqueue(neighbor);
                }
            }
        }
        
        // --- 阶段 2: 生成向量场 ---
        for (var x = 0; x < _width; x++)
        {
            for (var y = 0; y < _height; y++)
            {
                var index = y * _width + x;
                
                // 如果是墙或者不可达区域
                if (_costField[index] == 255 || _integrationField[index] == int.MaxValue) 
                {
                    _vectorField[index] = Vector2.Zero;
                    continue;
                }

                var bestDist = _integrationField[index];
                var bestNeighbor = (x, y); 
                
                foreach (var n in GetNeighbors(x, y))
                {
                    var nIndex = n.y * _width + n.x;
                    
                    // 同样需要进行切角检查，防止向量指向穿墙方向
                    if (!CanMoveDiagonal(x, y, n.x, n.y)) continue;
                    
                    // 寻找积分更小的邻居
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
                    _vectorField[index] = GetDirection(bestNeighbor.x - x, bestNeighbor.y - y);
                }
            }
        }
    }

    /// <summary>
    /// 检查是否可以进行斜向移动（防止穿墙角）
    /// </summary>
    private bool CanMoveDiagonal(int currentX, int currentY, int targetX, int targetY)
    {
        // 如果是直线移动，直接允许（墙壁检查在外层做）
        if (currentX == targetX || currentY == targetY) return true;

        // 斜向移动：检查两个分量方向的格子是否阻塞
        // 例如：从 (1,1) 移动到 (0,0)，必须检查 (1,0) 和 (0,1) 是否为墙
        
        // 检查水平相邻格
        int horzIndex = currentY * _width + targetX;
        if (_costField[horzIndex] == 255) return false;

        // 检查垂直相邻格
        int vertIndex = targetY * _width + currentX;
        if (_costField[vertIndex] == 255) return false;

        return true;
    }

    public Vector2 GetDirection(int x, int y)
    {
        // 预计算的归一化向量，减少 Sqrt 开销
        return (x, y) switch
        {
            (1, 0) => Vector2.UnitX,
            (-1,0) => -Vector2.UnitX,
            (0, 1) => Vector2.UnitY,
            (0, -1) => -Vector2.UnitY,
            (1, 1) => new Vector2(0.70710678f, 0.70710678f),
            (1, -1)=> new Vector2(0.70710678f, -0.70710678f),
            (-1,-1)=> new Vector2(-0.70710678f, -0.70710678f),
            (-1, 1)=> new Vector2(-0.70710678f, 0.70710678f),
            _ => Vector2.Zero,
        };
    }
    
    public Vector2 GetFlowDirection(ref Vector2 worldPos)
    {
        var x = (int)(worldPos.X / _tileSize);
        var y = (int)(worldPos.Y / _tileSize);

        if (x < 0 || x >= _width || y < 0 || y >= _height) return Vector2.Zero;
    
        var index = y * _width + x;
        var dir = _vectorField[index];

        // 如果在死胡同或局部极小值，尝试逃逸（通常不需要，除非Dijkstra生成失败）
        if (dir == Vector2.Zero)
        {
             // 只有当不是目标点时才计算逃逸
             // 实际上如果有完善的积分场，这里很少会进去
            return GetEscapeDirection(x, y);
        }
    
        return dir;
    }

    private Vector2 GetEscapeDirection(int cx, int cy)
    {
        var bestDist = int.MaxValue;
        (int x, int y) bestNeighbor = (-1, -1);

        foreach (var n in GetNeighbors(cx, cy))
        {
            var idx = n.y * _width + n.x;
            
            // 同样加入切角检查
            if (!CanMoveDiagonal(cx, cy, n.x, n.y)) continue;

            if (_costField[idx] != 255 && _integrationField[idx] < bestDist)
            {
                bestDist = _integrationField[idx];
                bestNeighbor = n;
            }
        }
        
        if (bestNeighbor.x != -1)
        {
            return GetDirection(bestNeighbor.x - cx, bestNeighbor.y - cy);
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
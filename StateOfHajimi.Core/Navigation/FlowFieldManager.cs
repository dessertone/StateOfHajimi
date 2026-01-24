using System.Collections.Concurrent;
using System.Numerics;
using Serilog;
using StateOfHajimi.Core.Enums;
using StateOfHajimi.Core.Maths;
using StateOfHajimi.Engine.Data;

namespace StateOfHajimi.Core.Navigation;

public class FlowFieldManager
{

    public static FlowFieldManager Instance { get; } = new FlowFieldManager();

    private TileMap _currentMap;
    

    private readonly ConcurrentDictionary<(int, int), FlowField> _activeFields = new();

    private readonly ConcurrentQueue<FlowField> _pool = new();

    private FlowFieldManager() { }


    public void Initialize(TileMap map)
    {
        _currentMap = map;
        Clear();
        Log.Information($"FlowFieldManager initialized with map size: {map.Width}x{map.Height}");
    }

    public FlowField GetFlowField(ref Vector2 targetWorldPos)
    {
        if (_currentMap == null)
        {
            Log.Error("FlowFieldManager not initialized with a map!");
            return null;
        }

        var tileX = (int)(targetWorldPos.X / _currentMap.TileSize);
        var tileY = (int)(targetWorldPos.Y / _currentMap.TileSize);

        if (tileX < 0 || tileX >= _currentMap.Width || tileY < 0 || tileY >= _currentMap.Height)
        {
            return null;
        }

        var key = (tileX, tileY);
        
        if (_activeFields.TryGetValue(key, out var cachedField))
        {
            return cachedField;
        }
        
        if (!_pool.TryDequeue(out var field))
        {
            field = new FlowField(_currentMap);
        }
        
        field.Generate(targetWorldPos);
        
        _activeFields.TryAdd(key, field);
        
        return field;
    }
    
    public void Clear()
    {
        foreach (var field in _activeFields.Values)
        {
            _pool.Enqueue(field);
        }
        _activeFields.Clear();
    }
    

    public void RecycleUnused()
    {
        Clear(); 
    }
}
using System.Numerics;
using Arch.Core;
using Arch.Core.Extensions;
using WarInPalace.Core.Components;
using WarInPalace.Core.Data;
using WarInPalace.Core.Enums;
using WarInPalace.Core.Utils;

namespace WarInPalace.Core.Systems;

public class SelectionSystem : BaseSystem
{
    // 用于清理旧选择的 Query
    private static readonly QueryDescription _selectedQuery = new QueryDescription()
        .WithAll<IsSelected>();


    private const float DragThresholdSq = 16.0f; 
    
    private const float ClickSensitivity = 5.0f;

    private readonly InputState _inputState = InputState.Instance;
    private readonly SpatialGrid _spatialGrid = SpatialGrid.Instance; 

    public SelectionSystem(World world) : base(world)
    {
    }

    public override void Update(float deltaTime)
    {
        if (!_inputState.IsNewSelectionTriggered) return;
        
        _inputState.IsNewSelectionTriggered = false;
        
        var start = _inputState.DragStartPosition;
        var end = _inputState.DragEndPosition;
        
        float distSq = Vector2.DistanceSquared(start, end);
        bool isSingleClick = distSq < DragThresholdSq;

        // 清除当前所有已选单位
        // TODO: 尚未实现Shift加选
        GameWorld.Query(in _selectedQuery, (entity) =>
        {
            entity.Remove<IsSelected>();
        });

        // 确定查询范围
        Vector2 searchMin, searchMax;
        if (isSingleClick)
        {
            searchMin = end - new Vector2(ClickSensitivity);
            searchMax = end + new Vector2(ClickSensitivity);
        }
        else
        {
            searchMin = Vector2.Min(start, end);
            searchMax = Vector2.Max(start, end);
        }

        // 第一遍：空间网格粗略查询
        foreach (var entity in _spatialGrid.QueryRect(searchMin, searchMax))
        {
            // 第二遍：精确筛选
            if (!entity.IsAlive() || !entity.Has<Position>() || !entity.Has<BodyCollider>()) 
                continue;
            
            ref var pos = ref entity.Get<Position>();
            ref var collider = ref entity.Get<BodyCollider>();
            bool shouldSelect;
            if (isSingleClick)
            {
                shouldSelect = IsPointInCollider(end, pos.Value, collider);
                //有多个重叠，都会被选中
            }
            else
            {
                
                shouldSelect = pos.Value.X >= searchMin.X && pos.Value.X <= searchMax.X &&
                               pos.Value.Y >= searchMin.Y && pos.Value.Y <= searchMax.Y;
            }
            if (shouldSelect)
            {
                if (!entity.Has<IsSelected>())
                {
                    entity.Add(new IsSelected());
                }
            }
        }
    }


    /// <summary>
    /// 判断点是否在碰撞体内
    /// </summary>
    /// <param name="point"></param>
    /// <param name="unitPos"></param>
    /// <param name="collider"></param>
    /// <returns></returns>
    private bool IsPointInCollider(Vector2 point, Vector2 unitPos, BodyCollider collider)
    {
        Vector2 center = unitPos + collider.Offset;

        if (collider.Type == BodyType.Circle)
        {
            
            return Vector2.DistanceSquared(point, center) <= (collider.Size.X * collider.Size.X);
        }
        else if (collider.Type == BodyType.AABB)
        {
            
            float halfW = collider.Size.X;
            float halfH = collider.Size.Y;
            return point.X >= center.X - halfW && point.X <= center.X + halfW &&
                   point.Y >= center.Y - halfH && point.Y <= center.Y + halfH;
        }
        return false;
    }
}
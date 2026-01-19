using System.Numerics;
using Arch.Buffer;
using Arch.Core;
using Arch.Core.Extensions;
using StateOfHajimi.Core.Components.MoveComponents;
using StateOfHajimi.Core.Components.Tags;
using StateOfHajimi.Core.Enums;
using StateOfHajimi.Core.Maths;
using StateOfHajimi.Core.Systems.Input.Commands;
using StateOfHajimi.Core.Utils;
using StateOfHajimi.Core.Utils.Attributes;

namespace StateOfHajimi.Core.Systems.Input.CommandHandlers;

[CommandType(nameof(SelectCommand))]
public class SelectHandler : ICommandHandler
{

    private static readonly QueryDescription _selectedQuery = new QueryDescription().WithAll<IsSelected>();

    private const float DragThresholdSq = 16.0f; 

    private const float ClickSensitivity = 15.0f;

    private readonly SpatialGrid _spatialGrid = SpatialGrid.Instance;

    private readonly List<Entity> _candidates = new(128);

    public void Handle(CommandBuffer buffer, GameCommand command, World world, float deltaTime)
    {
        if (command is not SelectCommand selectCommand) return;

        var start = selectCommand.Start;
        var end = selectCommand.End;
        
        // var isAdditive = selectCommand.IsShiftPressed; 
        var isAdditive = false; 

        var distSq = Vector2.DistanceSquared(start, end);
        var isSingleClick = distSq < DragThresholdSq;

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
        _candidates.Clear();
        foreach (var entity in _spatialGrid.QueryRect(searchMin, searchMax))
        {
            if (!IsSelectableEntity(entity)) continue;
            ref var pos = ref entity.Get<Position>();
            ref var collider = ref entity.Get<BodyCollider>();
            if(!isSingleClick&& entity.Has<BuildingClass>()) continue;
            var isHit = false;
            if (isSingleClick)
            {
                isHit = IsPointInCollider(end, pos.Value, in collider);
            }
            else
            {

                var selectionAABB = new AABB(searchMin, searchMax);
                isHit = Intersects(selectionAABB, pos.Value, in collider);
            }

            if (isHit)
            {
                _candidates.Add(entity);
            }
        }
        
        if (isSingleClick)
        {
            HandleSingleClickSelection(buffer, world, end, isAdditive);
        }
        else
        {
            HandleBoxSelection(buffer, world, isAdditive);
        }
    }
    
    private void HandleSingleClickSelection(CommandBuffer buffer, World world, Vector2 clickPoint, bool isAdditive)
    {
        if (_candidates.Count == 0)
        {
            if (!isAdditive) ClearSelection(buffer, world);
            return;
        }
        
        var bestTarget = _candidates[0];
        var bestScore = float.MinValue;

        foreach (var entity in _candidates)
        {
            ref var pos = ref entity.Get<Position>();

            var score = pos.Value.Y * 1000f - Vector2.DistanceSquared(pos.Value, clickPoint);
            
            if (score > bestScore)
            {
                bestScore = score;
                bestTarget = entity;
            }
        }
        
        if (!isAdditive)
        {
            ClearSelection(buffer, world);
        }
        
        if (!bestTarget.Has<IsSelected>())
        {
            buffer.Add<IsSelected>(bestTarget);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    private void HandleBoxSelection(CommandBuffer buffer, World world, bool isAdditive)
    {
        var candidateSet = new HashSet<Entity>(_candidates);
        if (!isAdditive)
        {
            world.Query(in _selectedQuery, (entity) =>
            {
                if (!candidateSet.Contains(entity))
                {
                    buffer.Remove<IsSelected>(entity);
                }
            });
        }
        foreach (var entity in _candidates)
        {
            if (!entity.Has<IsSelected>())
            {
                buffer.Add<IsSelected>(entity);
            }
        }
    }

    private void ClearSelection(CommandBuffer buffer, World world)
    {
        world.Query(in _selectedQuery, (entity) =>
        {
            buffer.Remove<IsSelected>(entity);
        });
    }

    private bool IsSelectableEntity(Entity entity)
    {
        return entity.IsAlive() && 
               entity.Has<Position>() && 
               entity.Has<BodyCollider,Selectable>() && 
               !entity.Has<Disabled,IsDying>();    
    }
    
    
    private bool IsPointInCollider(Vector2 point, Vector2 pos, in BodyCollider collider)
    {
        Vector2 center = pos + collider.Offset;
        if (collider.Type == BodyType.Circle)
        {
            return Vector2.DistanceSquared(point, center) <= (collider.Size.X * collider.Size.X);
        }
        // AABB
        var halfW = collider.Size.X;
        var halfH = collider.Size.Y;
        return point.X >= center.X - halfW && point.X <= center.X + halfW &&
               point.Y >= center.Y - halfH && point.Y <= center.Y + halfH;
    }
    
    private bool Intersects(AABB selectionBox, Vector2 pos, in BodyCollider collider)
    {
        Vector2 center = pos + collider.Offset;

        if (collider.Type == BodyType.AABB)
        {
            // AABB vs AABB
            var otherMin = new Vector2(center.X - collider.Size.X, center.Y - collider.Size.Y);
            var otherMax = new Vector2(center.X + collider.Size.X, center.Y + collider.Size.Y);
            
            return selectionBox.Min.X <= otherMax.X && selectionBox.Max.X >= otherMin.X &&
                   selectionBox.Min.Y <= otherMax.Y && selectionBox.Max.Y >= otherMin.Y;
        }
        else if (collider.Type == BodyType.Circle)
        {

            var closestX = Math.Clamp(center.X, selectionBox.Min.X, selectionBox.Max.X);
            var closestY = Math.Clamp(center.Y, selectionBox.Min.Y, selectionBox.Max.Y);

            var distanceX = center.X - closestX;
            var distanceY = center.Y - closestY;
            var distanceSquared = (distanceX * distanceX) + (distanceY * distanceY);

            return distanceSquared < (collider.Size.X * collider.Size.X);
        }
        return false;
    }
    
    private readonly struct AABB
    {
        public readonly Vector2 Min;
        public readonly Vector2 Max;
        public AABB(Vector2 min, Vector2 max) { Min = min; Max = max; }
    }
}
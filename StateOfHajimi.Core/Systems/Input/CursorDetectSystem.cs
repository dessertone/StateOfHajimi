using System.Numerics;
using Arch.Core;
using Arch.Core.Extensions;
using Arch.System;
using StateOfHajimi.Core.Components.MoveComponents;
using StateOfHajimi.Core.Components.RenderComponents;
using StateOfHajimi.Core.Components.StateComponents;
using StateOfHajimi.Core.Components.Tags;
using StateOfHajimi.Core.Enums;
using StateOfHajimi.Core.Maths;
using StateOfHajimi.Engine.Enums;
using StateOfHajimi.Engine.Input;

namespace StateOfHajimi.Core.Systems.Input;

public class CursorDetectSystem : BaseSystem<World, float>
{
    private readonly IBridge _bridge;
    private Entity _lastHoveredEntity = Entity.Null;

    public CursorDetectSystem(World world, IBridge bridge) : base(world)
    {
        _bridge = bridge;
    }

    public override void Update(in float t)
    {
        var mousePos = _bridge.CurSnapshot.MouseWorldPosition;
        var bestCandidate = Entity.Null;
        var maxSortY = float.MinValue;
        
        var candidates = SpatialGrid.Instance.Retrieve(mousePos);
        foreach (var entity in candidates)
        {
            if (!entity.IsAlive() || 
                !entity.Has<Position, RenderSize, Selectable, BodyCollider>() || 
                entity.Has<Disabled>() || 
                entity.Has<IsDying>()) continue;
            
            ref var body = ref entity.Get<BodyCollider>();
            ref var pos = ref entity.Get<Position>();
            ref var size = ref entity.Get<RenderSize>();
            
            if (IsPointInRenderBox(mousePos, pos.Value + body.Offset, size.Value))
            {
                if (pos.Value.Y > maxSortY)
                {
                    maxSortY = pos.Value.Y;
                    bestCandidate = entity;
                }
            }
        }
        if (bestCandidate == _lastHoveredEntity)
        {
            if (bestCandidate != Entity.Null)
            {
                _bridge.IsHovering = true;
                _bridge.HoveredEntity = bestCandidate;
            }
            else
            {
                _bridge.IsHovering = false;
                _bridge.HoveredEntity = Entity.Null;
            }
            return;
        }
        if (_lastHoveredEntity != Entity.Null && _lastHoveredEntity.IsAlive())
        {
            if (_lastHoveredEntity.Has<IsHovered>())
            {
                _lastHoveredEntity.Remove<IsHovered>();
            }
        }
        if (bestCandidate != Entity.Null)
        {
            bestCandidate.Add<IsHovered>();
            
            _bridge.IsHovering = true;
            _bridge.HoveredEntity = bestCandidate;
            _bridge.CursorHoverType = ResolveHoverType(bestCandidate);
        }
        else
        {
            _bridge.IsHovering = false;
            _bridge.HoveredEntity = Entity.Null;
            _bridge.CursorHoverType = HoverType.None;
        }
        _lastHoveredEntity = bestCandidate;
    }

    private bool IsPointInRenderBox(Vector2 point, Vector2 pos, Vector2 size)
    {
        var halfW = size.X * 0.5f;
        var halfH = size.Y * 0.5f;
        return point.X >= pos.X - halfW && point.X <= pos.X + halfW &&
               point.Y >= pos.Y - halfH && point.Y <= pos.Y + halfH;
    }

    private HoverType ResolveHoverType(Entity entity)
    {
        if (entity.Has<TeamId>() && entity.Get<TeamId>().Value != 0) 
            return HoverType.Opponent;
        return HoverType.Friend;
    }
}
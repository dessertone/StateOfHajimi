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


namespace StateOfHajimi.Core.Systems.Input;

public class CursorDetectSystem : BufferBaseSystem
{
    private readonly InputBridge _bridge;
    private Entity _lastHoveredEntity = Entity.Null;

    public CursorDetectSystem(World world, InputBridge bridge) : base(world)
    {
        _bridge = bridge;
    }

    public override void Update(in float t)
    {
        CleanUpPreviousFrame();

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
        if (bestCandidate != Entity.Null)
        {
            ApplyHoverState(bestCandidate);
        }
    }

    private void CleanUpPreviousFrame()
    {
        _bridge.CursorHoverType = HoverType.None;
        _bridge.HoveredEntity = Entity.Null;
        _bridge.IsHovering = false;
        if (_lastHoveredEntity != Entity.Null && _lastHoveredEntity.IsAlive())
        {
            if (_lastHoveredEntity.Has<IsHovered>())
            {
                Buffer.Remove<IsHovered>(_lastHoveredEntity);
            }
        }
        _lastHoveredEntity = Entity.Null;
    }

    private bool IsPointInRenderBox(Vector2 point, Vector2 pos, Vector2 size)
    {
        var halfW = size.X * 0.5f;
        var halfH = size.Y * 0.5f;

        var minX = pos.X - halfW;
        var maxX = pos.X + halfW;
        var minY = pos.Y - halfH;
        var maxY = pos.Y + halfH;

        return point.X >= minX && point.X <= maxX &&
               point.Y >= minY && point.Y <= maxY;
    }

    private void ApplyHoverState(Entity entity)
    {
        _lastHoveredEntity = entity;
        Buffer.Add<IsHovered>(entity);
        _bridge.HoveredEntity = entity;
        _bridge.CursorHoverType = ResolveHoverType(entity);
        _bridge.IsHovering = true;
    }

    private HoverType ResolveHoverType(Entity entity)
    {
        if (entity.Has<TeamId>() && entity.Get<TeamId>().Value != 0) 
            return HoverType.Opponent;
        return HoverType.Friend;
    }
}
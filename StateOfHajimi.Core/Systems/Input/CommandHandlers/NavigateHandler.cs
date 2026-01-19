using System.Buffers;
using System.Numerics;
using Arch.Buffer;
using Arch.Core;
using StateOfHajimi.Core.Components.MoveComponents;
using StateOfHajimi.Core.Components.PathComponents;
using StateOfHajimi.Core.Components.Tags;
using StateOfHajimi.Core.Enums;
using StateOfHajimi.Core.Navigation;
using StateOfHajimi.Core.Systems.Input.Commands;
using StateOfHajimi.Core.Utils.Attributes;
using StateOfHajimi.Core.Utils.FormationGenerators;

namespace StateOfHajimi.Core.Systems.Input.CommandHandlers;

[CommandType(nameof(NavigateCommand))]
public class NavigateHandler: ICommandHandler
{
    private static readonly QueryDescription _queryDescription = new QueryDescription()
        .WithAll<Position, Destination, IsSelected, Selectable>()
        .WithNone<Disabled, IsDying>();
    
public void Handle(CommandBuffer buffer, GameCommand command, World world, float deltaTime)
{
    if (command is not NavigateCommand navigateCommand) return;
    var units = new List<(Entity Entity, Vector2 Pos)>(64);
    world.Query(in _queryDescription, (Entity entity, ref Position position) =>
    {
        units.Add((entity, position.Value));
    });

    int count = units.Count;
    if (count == 0) return;

    var target = navigateCommand.target;
    var flowField = FlowFieldManager.Instance.GetFlowField(ref target);
    if (flowField == null) return;
    
    var targetPoints = ArrayPool<Vector2>.Shared.Rent(count);
    try 
    {
        var formation = FormationFactory.Get(FormationType.Rectangle);
        using var generator = formation.Spawn(navigateCommand.target, spacing: 70).GetEnumerator();
        
        int tIdx = 0;
        while(tIdx < count && generator.MoveNext())
        {
            targetPoints[tIdx++] = generator.Current;
        }
        
        var posComparer = new PositionComparer(); 

        units.Sort(posComparer);

        Array.Sort(targetPoints, 0, count, new Vector2Comparer());
        for (int i = 0; i < count; i++)
        {
            var entity = units[i].Entity;
            var targetPos = targetPoints[i];

            ref var currentDest = ref world.Get<Destination>(entity);

            if (currentDest.Value != targetPos || !currentDest.IsActive)
            {
                buffer.Set(entity, currentDest with { Value = targetPos, IsActive = true });
            }
            if (world.Has<FlowAlgorithm>(entity))
            {
                ref var flow = ref world.Get<FlowAlgorithm>(entity);
                flow.FlowField = flowField;
                flow.IsActive = true;
            }
            else
            {
                buffer.Add(entity, new FlowAlgorithm { FlowField = flowField, IsActive = true });
            }
        }
    }
    finally
    {
        ArrayPool<Vector2>.Shared.Return(targetPoints);
    }
}

private struct PositionComparer : IComparer<(Entity Entity, Vector2 Pos)>
{
    public int Compare((Entity Entity, Vector2 Pos) a, (Entity Entity, Vector2 Pos) b)
    {
        // 先排 Y (行)，再排 X (列)，适合 RTS 矩形阵型
        int yComp = a.Pos.Y.CompareTo(b.Pos.Y);
        if (yComp != 0) return yComp;
        return a.Pos.X.CompareTo(b.Pos.X);
    }
}

private struct Vector2Comparer : IComparer<Vector2>
{
    public int Compare(Vector2 a, Vector2 b)
    {
        int yComp = a.Y.CompareTo(b.Y);
        if (yComp != 0) return yComp;
        return a.X.CompareTo(b.X);
    }
}
}
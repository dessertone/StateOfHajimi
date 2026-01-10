using System.Numerics;
using Arch.Core;
using WarInPalace.Core.Components;
using WarInPalace.Core.Components.PathComponents;
using WarInPalace.Core.Components.Tags;
using WarInPalace.Core.Enums;
using WarInPalace.Core.Components.MoveComponents;
using WarInPalace.Core.Components.PathComponents;
using WarInPalace.Core.Components.Tags;
using WarInPalace.Core.Enums;
using WarInPalace.Core.Input.Commands;
using WarInPalace.Core.Utils.Attributes;
using WarInPalace.Core.Utils.FormationGenerators;

namespace WarInPalace.Core.Input.CommandHandlers;

[CommandType(nameof(NavigateCommand))]
public class NavigateHandler: ICommandHandler
{
    private static readonly QueryDescription _queryDescription = new QueryDescription()
        .WithAll<Position, Destination, IsSelected>();
    
    public World GameWorld { get; } 

    public void Handle(GameCommand command, World world, float deltaTime)
    {
        if (command is not NavigateCommand navigateCommand) 
            throw new ArgumentException("command is not navigateCommand");
        
        var units = new List<(Entity Entity, Vector2 Pos)>();

        world.Query(in _queryDescription, (Entity entity, ref Position position) =>
        {
            units.Add((entity, position.Value));
        });

        if (units.Count == 0) return;
        
        var formation = FormationResolver.Get(FormationType.Spiral);
        var targetPoints = new List<Vector2>();
        using var generator = formation.Spawn(navigateCommand.target, spacing: 70).GetEnumerator();
        for (var i = 0; i < units.Count; i++)
        {
            if (generator.MoveNext())
            {
                targetPoints.Add(generator.Current);
            }
        }
        
        var assignments = SolveGreedyAssignment(units, targetPoints);
        foreach (var assignment in assignments)
        {
            ref var currentDest = ref world.Get<Destination>(assignment.Entity);
            currentDest.Value = assignment.TargetPos;
            currentDest.IsActive = true;
        }
    }

    /// <summary>
    /// 贪心分配
    /// </summary>
    private List<(Entity Entity, Vector2 TargetPos)> SolveGreedyAssignment(
        List<(Entity Entity, Vector2 Pos)> units, 
        List<Vector2> targets)
    {
        var result = new List<(Entity, Vector2)>();
        var pairs = new List<(int uIndex, int tIndex, float distSq)>(units.Count * targets.Count);
        for (var u = 0; u < units.Count; u++)
        {
            for (var t = 0; t < targets.Count; t++)
            {
                var d2 = Vector2.DistanceSquared(units[u].Pos, targets[t]);
                pairs.Add((u, t, d2));
            }
        }
        
        pairs.Sort((a, b) => a.distSq.CompareTo(b.distSq));
        
        var usedUnits = new bool[units.Count];
        var usedTargets = new bool[targets.Count];
        var matchCount = 0;

        foreach (var p in pairs)
        {
            if (matchCount >= units.Count) break;
            if (!usedUnits[p.uIndex] && !usedTargets[p.tIndex])
            {
                usedUnits[p.uIndex] = true;
                usedTargets[p.tIndex] = true;
                result.Add((units[p.uIndex].Entity, targets[p.tIndex]));
                matchCount++;
            }
        }
        return result;
    }
}
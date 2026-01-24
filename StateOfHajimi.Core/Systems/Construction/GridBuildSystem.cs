using Arch.Core;
using Arch.Core.Extensions;
using Arch.System;
using Arch.System.SourceGenerator;
using Serilog;
using StateOfHajimi.Core.Components.MoveComponents;
using StateOfHajimi.Core.Components.Tags;
using StateOfHajimi.Core.Maths;
using StateOfHajimi.Core.Utils;

namespace StateOfHajimi.Core.Systems.Construction;

public partial class GridBuildSystem: BaseSystem<World, float>
{
    
    private static readonly SpatialGrid _spatialGrid = SpatialGrid.Instance; 
    public GridBuildSystem(World world) : base(world) { }
    [Query]
    [All<BodyCollider, Position>, None<Disabled,IsDying>]
    private void BuildGrid(Entity entity, ref Position pos)
    {
        _spatialGrid.Add(entity, pos.Value);
    }
    
    public override void BeforeUpdate(in float t)
    {
        base.BeforeUpdate(in t);
        _spatialGrid.Clear();
    }
}
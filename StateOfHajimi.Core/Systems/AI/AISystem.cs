using Arch.Core;
using Arch.Core.Extensions;
using Arch.System;
using Arch.System.SourceGenerator;
using StateOfHajimi.Core.Components.MoveComponents;
using StateOfHajimi.Core.Components.PathComponents;
using StateOfHajimi.Core.Components.StateComponents;
using StateOfHajimi.Core.Components.Tags;

namespace StateOfHajimi.Core.Systems.AI;

public partial class AISystem: BaseSystem<World, float>
{

    public AISystem(World world) : base(world) { }


    [Query]
    [All<AIController, Position, Health>, None<Disabled,IsDying>]
    public void ExecuteBehavior([Data] in float deltaTime, Entity entity, ref AIController ai, ref Health hp)
    {
        if (hp.IsDead) return;
        ai.RootNode.Execute(entity, deltaTime);
    }
}
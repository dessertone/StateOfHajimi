using System.Numerics;
using Arch.Core;
using Arch.System;
using Arch.System.SourceGenerator;
using StateOfHajimi.Core.Components.MoveComponents;
using StateOfHajimi.Core.Components.ProductComponents;
using StateOfHajimi.Core.Components.StateComponents;
using StateOfHajimi.Core.Components.Tags;
using StateOfHajimi.Core.Data.Builders.Bases;
using StateOfHajimi.Core.Enums;
using StateOfHajimi.Core.Navigation;

namespace StateOfHajimi.Core.Systems.Production;


public partial class AutoProductSystem:BufferBaseSystem
{
    
    public AutoProductSystem(World world) : base(world) { }

    public override void Initialize()
    {
        base.Initialize();
        /*var rally = new RallyPoint { IsSet = false, Target = new Vector2(5000, 5000)};
        var context = new BuildContext(new Vector2(3000,6000),0, rally);
        UnitFactory.CreateEntity(Buffer, EntityType.LittleHajimi, ref context);*/
    }

    [Query]
    [All<Position, AutoProduction, TeamId, isProductionEnabled>,None<Disabled,IsDying>]
    public void AutoProduct([Data] in float deltaTime, ref Position pos, ref AutoProduction prod, ref TeamId teamId)
    {
        prod.Progress += deltaTime;
        if (prod.Progress >= prod.Interval)
        {
            prod.Progress = 0;
            var spawnPos = pos.Value + new Vector2(0, 300);
            var context = new BuildContext(spawnPos, teamId.Value, prod.Rally);

            UnitFactory.CreateEntity(Buffer, EntityType.LittleHajimi, ref context);
        }
    }
    
}
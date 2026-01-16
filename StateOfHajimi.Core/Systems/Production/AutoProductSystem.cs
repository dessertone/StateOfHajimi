using System.Numerics;
using Arch.Core;
using Arch.System;
using Arch.System.SourceGenerator;
using StateOfHajimi.Core.Components.MoveComponents;
using StateOfHajimi.Core.Components.ProductComponents;
using StateOfHajimi.Core.Components.StateComponents;
using StateOfHajimi.Core.Components.Tags;
using StateOfHajimi.Core.Data.EntityBuilders;
using StateOfHajimi.Core.Enums;

namespace StateOfHajimi.Core.Systems.Production;


public partial class AutoProductSystem:BufferBaseSystem
{
    
    public AutoProductSystem(World world) : base(world) { }

    public override void Initialize()
    {
        base.Initialize();
        /*var rally = new RallyPoint { IsSet = false, Target = new Vector2(5000, 5000)};
        UnitFactory.Create(Buffer, EntityType.LittleHajimi, new Vector2(6000,6000), 0,ref rally);
        UnitFactory.Create(Buffer, EntityType.LittleHajimi, new Vector2(6000,7000), 0,ref rally);
        UnitFactory.Create(Buffer, EntityType.LittleHajimi, new Vector2(6000,8000), 0,ref rally);
        UnitFactory.Create(Buffer, EntityType.LittleHajimi, new Vector2(6000,9000), 0,ref rally);
        UnitFactory.Create(Buffer, EntityType.LittleHajimi, new Vector2(7000,6000), 1,ref rally);
        UnitFactory.Create(Buffer, EntityType.LittleHajimi, new Vector2(7000,7000), 1,ref rally);
        UnitFactory.Create(Buffer, EntityType.LittleHajimi, new Vector2(7000,5000), 1,ref rally);
        UnitFactory.Create(Buffer, EntityType.LittleHajimi, new Vector2(7000,4000), 1,ref rally);*/
        
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
            UnitFactory.Create(Buffer, EntityType.LittleHajimi, spawnPos, teamId.Value, ref prod.Rally);
        }
    }



}
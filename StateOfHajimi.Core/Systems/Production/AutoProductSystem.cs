using System.Numerics;
using Arch.Core;
using StateOfHajimi.Core.Components.MoveComponents;
using StateOfHajimi.Core.Components.ProductComponents;
using StateOfHajimi.Core.Components.StateComponents;
using StateOfHajimi.Core.Components.Tags;
using StateOfHajimi.Core.Data.EntityBuilders;

namespace StateOfHajimi.Core.Systems.Production;

public class AutoProductSystem:BaseSystem
{
    private static readonly QueryDescription _productQuery = new QueryDescription()
        .WithAll<Position, AutoProduction, isProductionEnabled, TeamId>();
    
    public AutoProductSystem(World world) : base(world)
    {
    }

    public override void Update(float deltaTime)
    {
        GameWorld.Query(in _productQuery, (ref Position pos,ref AutoProduction prod, ref TeamId teamId) =>
        {
            prod.Progress += deltaTime;
            if (prod.Progress >= prod.Interval)
            {
                prod.Progress = 0;
                var spawnPos = pos.Value + new Vector2(0, 600); 
                UnitFactory.CreateUnit(GameWorld, prod.ProductUnitType, spawnPos, teamId.Value);
            }
        });
    }
    
}
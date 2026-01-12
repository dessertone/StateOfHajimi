using System.Numerics;
using Arch.Core;
using StateOfHajimi.Core.AI.Base;
using StateOfHajimi.Core.AI.Nodes;
using StateOfHajimi.Core.Components.CombatComponents;
using StateOfHajimi.Core.Components.MoveComponents;
using StateOfHajimi.Core.Components.PathComponents;
using StateOfHajimi.Core.Components.ProductComponents;
using StateOfHajimi.Core.Components.StateComponents;
using StateOfHajimi.Core.Components.Tags;
using StateOfHajimi.Core.Enums;
using StateOfHajimi.Core.Utils.Attributes;

namespace StateOfHajimi.Core.Data.EntityBuilders;

[BuildUnitType(EntityType.LightweightMechanicalCat)]
public class LightweightMechanicalCatBuilder: IEntityBuilder
{
    public void Create(World world, Vector2 position, int teamId, ref RallyPoint rally)
    {

        var dest = rally.IsSet
            ? new Destination { StopDistanceSquared = 1.0f, Value = rally.Target, IsActive = true }
                : new Destination { StopDistanceSquared = 1.0f };
        var config = GameConfig.GetUnitState(EntityType.LightweightMechanicalCat);
        world.Create(
            new AIController
            {
                RootNode = new Selector(new()
                {
                    new Sequence(new()
                    {
                        new CheckTargetNode(),
                        new MoveToAttackRangeNode(),
                        new AttackNode()
                    }),
                    new IdleNode()
                })
            },
            new Position { Value = position },
            new Velocity { Value = Vector2.Zero },
            dest, 
            new AttackTarget{Target = Entity.Null},
            new CombatStats
            {
                AttackDamage = config.AttackDamage,
                AttackRange = config.AttackRange,
                AttackSpeed = config.AttackSpeed,
                VisionRange = config.VisionRange
            },
            new EntityClass{ Type = EntityType.LightweightMechanicalCat},
            new TeamId { Value = teamId },
            new MoveSpeed(config.MoveSpeed),
            BodyCollider.CreateCircle(config.Size, force: config.MoveSpeed / 1e4f),
            new Health
            {
                MaxHp = config.MaxHp,
                Current = config.MaxHp
            },
            new Selectable()
        );
    }
}
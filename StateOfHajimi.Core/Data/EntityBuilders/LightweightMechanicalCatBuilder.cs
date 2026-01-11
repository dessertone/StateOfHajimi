using System.Numerics;
using Arch.Core;
using StateOfHajimi.Core.Components.MoveComponents;
using StateOfHajimi.Core.Components.PathComponents;
using StateOfHajimi.Core.Components.StateComponents;
using StateOfHajimi.Core.Components.Tags;
using StateOfHajimi.Core.Enums;
using StateOfHajimi.Core.Utils.Attributes;

namespace StateOfHajimi.Core.Data.EntityBuilders;

[BuildUnitType(UnitType.LightweightMechanicalCat)]
public class LightweightMechanicalCatBuilder: IEntityBuilder
{
    public void Create(World world, Vector2 position, int teamId)
    {
        var config = GameConfig.GetUnitState(UnitType.LightweightMechanicalCat);
        world.Create(
            new Position { Value = position },
            new Velocity { Value = Vector2.Zero },
            new Destination{StopDistanceSquared = 1.0f}, 
            new EntityClass{ Type = UnitType.LightweightMechanicalCat},
            new TeamId { Value = teamId },
            new MoveSpeed(config.MoveSpeed),
            BodyCollider.CreateCircle(config.Size, force: config.MoveSpeed / 1e4f),
            new Health { MaxHp = config.MaxHp, Current = config.MaxHp },
            new Selectable()
        );
    }
}
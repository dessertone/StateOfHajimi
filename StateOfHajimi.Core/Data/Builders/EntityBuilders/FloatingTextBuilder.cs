using System.Numerics;
using Arch.Buffer;
using Arch.Core;
using StateOfHajimi.Core.Components.MoveComponents;
using StateOfHajimi.Core.Components.RenderComponents;
using StateOfHajimi.Core.Components.StateComponents;
using StateOfHajimi.Core.Components.Tags;
using StateOfHajimi.Core.Data.Builders.Bases;
using StateOfHajimi.Core.Enums;
using StateOfHajimi.Core.Utils.Attributes;

namespace StateOfHajimi.Core.Data.Builders.EntityBuilders;

[BuildUnitType(EntityType.FloatingText)]
public class FloatingTextBuilder:IEntityBuilder
{
    public ComponentType[] Archetype { get; } =
    [
        typeof(Position), typeof(Velocity), typeof(MoveSpeed), typeof(Text), typeof(Life), typeof(RenderSize)
    ];
    public void Build(CommandBuffer buffer, Entity entity, ref BuildContext context)
    {

        if (context.ExtraData?.Length < 3) return;
        var content = context.ExtraData?[0] as string ?? "Unknown";
        var velocity = (Vector2)(context.ExtraData?[1] ?? new Vector2(0 ,0));
        var vec = (Vector2)(context.ExtraData?[2] ?? new Vector2(0,0));
        
        buffer.Set(entity, new Text(content));
        buffer.Set(entity, new Velocity(velocity));
        buffer.Set(entity, new MoveSpeed(0.9f));
        buffer.Set(entity, new Life(2f));
        buffer.Set(entity, new Position(context.Position));
        buffer.Set(entity, new RenderSize(vec));
    }
}
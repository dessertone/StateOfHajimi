using System.Numerics;
using Arch.Buffer;
using Arch.Core;
using StateOfHajimi.Core.Components.MoveComponents;
using StateOfHajimi.Core.Components.ProductComponents;
using StateOfHajimi.Core.Components.RenderComponents;
using StateOfHajimi.Core.Components.StateComponents;
using StateOfHajimi.Core.Components.Tags;
using StateOfHajimi.Core.Data.Builders.Bases;
using StateOfHajimi.Core.Data.Config;
using StateOfHajimi.Core.Enums;
using StateOfHajimi.Core.Utils.Attributes;

namespace StateOfHajimi.Core.Data.Builders.BuildingBuilders;

[BuildUnitType(EntityType.LittleHajimiFactory)]
public class LittleHajimiFactoryBuilder:IEntityBuilder
{
    public ComponentType[] Archetype { get; } =
    [
        typeof(Position), typeof(Selectable), typeof(AutoProduction), typeof(AnimationState), typeof(isProductionEnabled),
        typeof(RenderSize), typeof(BodyCollider), typeof(BuildingClass), typeof(Health), typeof(TeamId)
    ];
    public void Build(CommandBuffer buffer, Entity entity, ref BuildContext context)
    {
    
        var config = GameConfig.GetUnitState(EntityType.LittleHajimiFactory);
        var animConfig = GameConfig.GetEntityAnimation(EntityType.LittleHajimiFactory);
        if(animConfig != null && animConfig.TryGetInfo(new AnimationKey(AnimationStateType.Running, Direction.None), out var state))
        {
            buffer.Set(entity, new AnimationState
            {
                AnimationTarget = EntityType.LittleHajimiFactory,
                IsLoop=state.IsLoop,
                StartFrame = state.StartFrame,
                EndFrame = state.EndFrame, 
                FrameDuration = state.FrameDuration,
                IsActive = true
            });
        }
        var rally = context.ExtraData is RallyPoint r ? r : new RallyPoint{IsSet = false};
        
        // 属性
        buffer.Set(entity, new Position(context.Position));
        buffer.Set(entity, new TeamId(context.TeamId));
        buffer.Set(entity, new BuildingClass{ Type = EntityType.LittleHajimiFactory});
        buffer.Set(entity, new Health
        {
            Current = config.MaxHp,
            MaxHp = config.MaxHp
        });
        // 标签
        buffer.Set(entity, new Selectable());
        buffer.Set(entity, new isProductionEnabled());
        
        // 渲染
        buffer.Set(entity, new BodyCollider 
        { 
            Type = BodyType.Circle, 
            Size = new Vector2(150, 0),
            AvoidanceForce = 999999f, 
            Offset = new Vector2(0, 150)
        });
        buffer.Set(entity, new RenderSize(new Vector2(600, 600)));

        
        // 行为
        buffer.Set(entity, new AutoProduction{Interval = 10f,
                ProductEntityType = EntityType.LittleHajimi, 
                Rally = rally,
                Progress = 10f
            });
    }
}
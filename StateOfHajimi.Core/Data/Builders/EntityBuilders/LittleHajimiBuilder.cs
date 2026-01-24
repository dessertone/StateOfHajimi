using System.Numerics;
using Arch.Buffer;
using Arch.Core;
using StateOfHajimi.Core.Components.CombatComponents;
using StateOfHajimi.Core.Components.MoveComponents;
using StateOfHajimi.Core.Components.PathComponents;
using StateOfHajimi.Core.Components.ProductComponents;
using StateOfHajimi.Core.Components.RenderComponents;
using StateOfHajimi.Core.Components.StateComponents;
using StateOfHajimi.Core.Components.Tags;
using StateOfHajimi.Core.Data.Builders.Bases;
using StateOfHajimi.Core.Data.Config;
using StateOfHajimi.Core.Enums;
using StateOfHajimi.Core.Maths;
using StateOfHajimi.Core.Navigation;
using StateOfHajimi.Core.Systems.AI.Base;
using StateOfHajimi.Core.Systems.AI.Nodes;
using StateOfHajimi.Core.Utils.Attributes;
using Parallel = StateOfHajimi.Core.Systems.AI.Base.Parallel;

namespace StateOfHajimi.Core.Data.Builders.EntityBuilders;

[BuildUnitType(EntityType.LittleHajimi)]
public class LittleHajimiBuilder : IEntityBuilder
{
    private static readonly ComponentType[] _archetype = 
    { 
        typeof(AIController), typeof(AnimationState), typeof(Position), 
        typeof(Velocity), typeof(Destination), typeof(AttackTarget), 
        typeof(CombatStats), typeof(EntityClass), typeof(TeamId), 
        typeof(MoveSpeed), typeof(BodyCollider), typeof(Health), 
        typeof(Selectable), typeof(RenderSize), typeof(Facing), typeof(FlowAlgorithm)
    };
    public ComponentType[] Archetype => _archetype;
    public void Build(CommandBuffer buffer, Entity entity,ref BuildContext context)
    {
        var position = context.Position;
        var teamId = context.TeamId;
        var rally = context.ExtraData?[0] is RallyPoint r ? r : new RallyPoint{IsSet = false};
        var flowField = rally.IsSet ? FlowFieldManager.Instance.GetFlowField(ref rally.Target) : null;
        // 准备实体数据 
        var config = GameConfig.GetUnitState(EntityType.LittleHajimi);
        
        // 动画
        var animConfig = GameConfig.GetEntityAnimation(EntityType.LittleHajimi);
        if (animConfig != null && animConfig.TryGetInfo(new AnimationKey(AnimationStateType.Idle, Direction.South), out var idleState))
        {
            // 渲染动画状态
            buffer.Set(entity, new AnimationState
            {
                FrameDuration = idleState.FrameDuration,
                StartFrame = idleState.StartFrame,
                EndFrame = idleState.EndFrame,
                IsActive = true,
                AnimationTarget = EntityType.LittleHajimi,
                Type = AnimationStateType.Idle,
                Offset = 0,
                FrameTimer = 0
            });
        }


        // 处理集结点逻辑
        var destination = rally.IsSet
            ? new Destination { StopDistanceSquared = 100.0f, Value = rally.Target, IsActive = true }
            : new Destination { StopDistanceSquared = 100.0f };

        // 构建行为树
        var behaviorTree = new AIController
        {
            RootNode = new Parallel([
                new Selector([
                    new NavigationNode(),
                    new IdleNode()        
                ]),
                new Sequence([
                    new CheckTargetNode(),
                    new AttackNode()       
                ])
            ])
        };

        // 设置基础属性
        buffer.Set(entity, new EntityClass { Type = EntityType.LittleHajimi });
        buffer.Set(entity, new TeamId { Value = teamId });
        buffer.Set(entity, new Selectable());
        buffer.Set(entity, behaviorTree); 
        buffer.Set(entity, new Facing(Direction.South));
        // 设置战斗属性 
        buffer.Set(entity, new Health 
        { 
            MaxHp = config.MaxHp, 
            Current = config.MaxHp 
        });
        
        buffer.Set(entity, new CombatStats
        {
            AttackDamage = config.AttackDamage,
            AttackRange = config.AttackRange,
            AttackSpeed = config.AttackSpeed,
            VisionRange = config.VisionRange,
            CurrentCooldown = 0
        });

        buffer.Set(entity, new AttackTarget { Target = Entity.Null });

        // 物理与移动
        buffer.Set(entity, new Position { Value = position });
        buffer.Set(entity, new Velocity { Value = Vector2.Zero });
        buffer.Set(entity, new MoveSpeed(config.MoveSpeed));
        buffer.Set(entity, destination);
        buffer.Set(entity, new RenderSize(new Vector2(115*0.5f,178*0.5f)));
        buffer.Set(entity, new BodyCollider
        {
            AvoidanceForce = 40f,
            Offset = new Vector2(0, 20),
            Size = new Vector2(config.Size, 0),
            Type = BodyType.Circle
        });
        
        buffer.Set(entity, new FlowAlgorithm(flowField, true));
    }
}
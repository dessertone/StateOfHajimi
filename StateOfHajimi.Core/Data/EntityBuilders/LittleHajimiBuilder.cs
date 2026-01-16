using System.Numerics;
using Arch.Buffer;
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

[BuildUnitType(EntityType.LittleHajimi)]
public class LittleHajimiBuilder : IEntityBuilder
{
    private static readonly ComponentType[] _archetype = 
    { 
        typeof(AIController), typeof(AnimationState), typeof(Position), 
        typeof(Velocity), typeof(Destination), typeof(AttackTarget), 
        typeof(CombatStats), typeof(EntityClass), typeof(TeamId), 
        typeof(MoveSpeed), typeof(BodyCollider), typeof(Health), 
        typeof(Selectable), typeof(RenderSize)
    };
    public ComponentType[] Archetype => _archetype;
    public void Build(CommandBuffer buffer, Entity entity, Vector2 position, int teamId, ref RallyPoint rally)
    {
        // 准备实体数据
        var config = GameConfig.GetUnitState(EntityType.LittleHajimi);
        // 动画
        var animConfig = GameConfig.GetEntityAnimation(EntityType.LittleHajimi);
        var idleState = animConfig?.StateAnimations[nameof(AnimationStateType.Idle)];


        // 处理集结点逻辑
        var destination = rally.IsSet
            ? new Destination { StopDistanceSquared = 3.0f, Value = rally.Target, IsActive = true }
            : new Destination { StopDistanceSquared = 3.0f };

        // 构建行为树
        var behaviorTree = new AIController
        {
            RootNode = new Selector([
                new Sequence([
                    new NavigationNode(),
                    new Selector(
                    [
                        new CheckTargetNode(),
                        new AttackNode(),
                        new SuccessNode()
                    ])]),
                new Sequence(
                [
                    new AggressiveSearchTargetNode(),
                    new MoveToAttackRangeNode(),
                    new AttackNode()
                ]),
                new IdleNode()
            ])
        };

        // 设置基础属性
        buffer.Set(entity, new EntityClass { Type = EntityType.LittleHajimi });
        buffer.Set(entity, new TeamId { Value = teamId });
        buffer.Set(entity, new Selectable());
        buffer.Set(entity, behaviorTree); 

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
        buffer.Set(entity, new RenderSize(new Vector2(115, 178)));
        buffer.Set(entity, new BodyCollider
        {
            AvoidanceForce = 1.2f,
            Offset = new Vector2(0, 80),
            Size = new Vector2(50, 0),
            Type = BodyType.Circle
        });

        // 渲染动画状态
        buffer.Set(entity, new AnimationState
        {
            FrameDuration = idleState.FrameDuration,
            StartFrame = idleState.StartFrame,
            EndFrame = idleState.EndFrame,
            IsActive = true,
            AnimationKey = nameof(EntityType.LittleHajimi),
            Type = AnimationStateType.Idle,
            Offset = 0,
            FrameTimer = 0
        });
    }
}
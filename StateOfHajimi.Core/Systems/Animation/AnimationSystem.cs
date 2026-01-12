using Arch.Core;
using StateOfHajimi.Core.Components.StateComponents;

namespace StateOfHajimi.Core.Systems.Animation;

public class AnimationSystem:BaseSystem
{
    
    private static readonly QueryDescription _animationQuery = new QueryDescription()
        .WithAll<AnimationState>();
    public AnimationSystem(World world) : base(world)
    {
    }

    public override void Update(float deltaTime)
    {
        PlayAnimation(deltaTime);
    }

    private void PlayAnimation(float deltaTime)
    {
        GameWorld.Query(in _animationQuery, (Entity entity, ref AnimationState animationState) =>
        {
            if (!animationState.IsActive) return;
            animationState.FrameTimer += deltaTime;
            if (!(animationState.FrameTimer >= animationState.FrameDuration)) return;
            animationState.FrameTimer -= animationState.FrameDuration;
            animationState.Offset = (animationState.Offset + 1) % (animationState.EndFrame - animationState.StartFrame);
        });
    }
}
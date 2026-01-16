using Arch.Core;
using Arch.System;
using Arch.System.SourceGenerator;
using StateOfHajimi.Core.Components.StateComponents;
using StateOfHajimi.Core.Components.Tags;

namespace StateOfHajimi.Core.Systems.Animation;

public partial class AnimationSystem:BaseSystem<World, float>
{
    
    public AnimationSystem(World world) : base(world) { }
    
    [Query]
    [All<AnimationState>, None<Disabled>]
    private void PlayAnimation([Data]in float deltaTime, ref AnimationState animationState)
    {
        if (!animationState.IsActive) return;
        animationState.FrameTimer += deltaTime;
        if (!(animationState.FrameTimer >= animationState.FrameDuration)) return;
        animationState.FrameTimer -= animationState.FrameDuration;
        var duration = animationState.EndFrame - animationState.StartFrame;
        if(duration != 0)
            animationState.Offset = (animationState.Offset + 1) % duration;
        if(!animationState.IsLoop && animationState.Offset == duration) animationState.IsActive = false;
    }
}
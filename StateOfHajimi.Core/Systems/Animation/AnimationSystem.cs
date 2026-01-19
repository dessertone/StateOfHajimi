using System.Numerics;
using Arch.Core;
using Arch.System;
using Arch.System.SourceGenerator;
using StateOfHajimi.Core.Components.MoveComponents;
using StateOfHajimi.Core.Components.RenderComponents;
using StateOfHajimi.Core.Components.StateComponents;
using StateOfHajimi.Core.Components.Tags;
using StateOfHajimi.Core.Utils.Extensions;

namespace StateOfHajimi.Core.Systems.Animation;

public partial class AnimationSystem:BaseSystem<World, float>
{
    
    public AnimationSystem(World world) : base(world) { }

    [Query]
    [All<AnimationState, Facing, Velocity>, None<Disabled>]
    private void UpdateDuration(ref AnimationState animationState, ref Velocity velocity, ref Facing facing)
    {
        if (velocity.Value != Vector2.Zero)
        {
            facing.Value = velocity.Value.Opposite();
            animationState.Switch(animationState.Type, facing.Value);
        }
    }
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
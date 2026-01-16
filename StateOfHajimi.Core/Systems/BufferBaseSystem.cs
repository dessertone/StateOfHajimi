using Arch.Buffer;
using Arch.Core;
using Arch.System;

namespace StateOfHajimi.Core.Systems;

public abstract class BufferBaseSystem:BaseSystem<World, float>
{
    protected CommandBuffer Buffer { get; } = new();
    
    public BufferBaseSystem(World world) : base(world)
    {
    }

    public override void AfterUpdate(in float t)
    {
        base.AfterUpdate(in t);
        Buffer.Playback(World);
    }

    public override void Dispose()
    {
        base.Dispose();
        Buffer.Dispose();
    }
}
using System.Diagnostics;
using Arch.Core;
using Arch.System;
using Serilog;
using StateOfHajimi.Core.Systems.Input.CommandHandlers;
using StateOfHajimi.Core.Utils;
using StateOfHajimi.Core.Utils.Attributes;

namespace StateOfHajimi.Core.Systems.Input;

public partial class CommandDispatchSystem:BufferBaseSystem
{
    private readonly InputSnapshot _snapshot;
    private readonly Dictionary<string, ICommandHandler> _commandHandlers;
    public CommandDispatchSystem(World world, InputSnapshot shot) : base(world)
    {
        _snapshot = shot;
        _commandHandlers = AttributeHelper.CommandHandlers;
    }
    public override void Update(in float deltaTime)
    {
        var stop = Stopwatch.StartNew();
        foreach (var command in _snapshot.Commands)
        {
            _commandHandlers[command.ToString()].Handle(Buffer, command, World, deltaTime);
        }
        var time = stop.Elapsed.TotalMilliseconds;
        if (time > 15)
        {
            Log.Warning($"LAG DETECTED! LOGIC TOOK: {time} ms");
        }
    }
}
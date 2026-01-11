using Arch.Core;
using StateOfHajimi.Core.Systems.Input.CommandHandlers;
using StateOfHajimi.Core.Utils;

namespace StateOfHajimi.Core.Systems.Input;

public class CommandDispatchSystem:BaseSystem
{
    private readonly InputSnapshot _snapshot;

    private readonly Dictionary<string, ICommandHandler> _commandHandlers;
    public CommandDispatchSystem(World world, InputSnapshot shot) : base(world)
    {
        _snapshot = shot;
        _commandHandlers = AttributeHelper.CommandHandlers;
    }
    
    public override void Update(float deltaTime)
    {
        foreach (var command in _snapshot.Commands)
        {
            _commandHandlers[command.ToString()].Handle(command, GameWorld, deltaTime);
        }
    }
}
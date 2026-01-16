using Arch.Buffer;
using Arch.Core;
using StateOfHajimi.Core.Systems.Input.Commands;

namespace StateOfHajimi.Core.Systems.Input.CommandHandlers;

public interface ICommandHandler
{
    void Handle(CommandBuffer buffer, GameCommand command, World world, float deltaTime);
}
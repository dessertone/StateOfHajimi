using Arch.Core;
using Arch.LowLevel;
using WarInPalace.Core.Input.Commands;

namespace WarInPalace.Core.Input.CommandHandlers;

public interface ICommandHandler
{
    void Handle(GameCommand command, World world, float deltaTime);
}
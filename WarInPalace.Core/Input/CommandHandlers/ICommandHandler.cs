using Arch.LowLevel;

namespace WarInPalace.Core.Input.CommandHandlers;

public interface ICommandHandler
{
    void Handle(GameCommand command);
}
using WarInPalace.Core.Utils.Attributes;

namespace WarInPalace.Core.Input.CommandHandlers;

[CommandType(nameof(SelectCommand))]
public class SelectHandler: ICommandHandler
{
    public void Handle(GameCommand command)
    {
        if(command is not SelectCommand selectCommand) throw new ArgumentException("command is not SelectCommand");
        
    }
}
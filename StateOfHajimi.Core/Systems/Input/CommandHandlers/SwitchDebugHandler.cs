using Arch.Buffer;
using Arch.Core;
using StateOfHajimi.Core.Components.GlobalComponents;
using StateOfHajimi.Core.Utils.Attributes;
using StateOfHajimi.Core.Utils.Extensions;
using StateOfHajimi.Engine.Input.Commands;

namespace StateOfHajimi.Core.Systems.Input.CommandHandlers;


[CommandType(nameof(SwitchDebugCommand))]
public class SwitchDebugHandler:ICommandHandler
{
    public void Handle(CommandBuffer buffer, GameCommand command, World world, float deltaTime)
    {
        if (command is not SwitchDebugCommand) return;
        var settings = world.GetSingleton<DebugSettings>();
        settings.ShowFlowField = !settings.ShowFlowField;
        settings.ShowColliderBox = !settings.ShowColliderBox;
        world.SetSingleton(settings);
    }
}
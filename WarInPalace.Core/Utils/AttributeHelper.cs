using System.Reflection;
using WarInPalace.Core.Enums;
using WarInPalace.Core.Input;
using WarInPalace.Core.Input.CommandHandlers;
using WarInPalace.Core.Utils.Attributes;
using WarInPalace.Core.Utils.FormationGenerators;

namespace WarInPalace.Core.Utils;

public static class AttributeHelper
{
    public static readonly Dictionary<string, ICommandHandler> CommandHandlers = new();
    public static readonly Dictionary<FormationType, IFormation> Strategies = new();
    

    private static Type[] types = Assembly.GetExecutingAssembly().GetTypes();

    public static void Initialize()
    {
        GetCommandHandlers();
        GetFormationStrategies();
    }
    public static void GetCommandHandlers()
    {
        var targetTypes = types.Where(t => t.IsAssignableTo(typeof(ICommandHandler)) && !t.IsAbstract && !t.IsInterface);
        foreach (var type in targetTypes)
        {
            if (type.GetCustomAttribute<CommandTypeAttribute>() is { } attr)
            {
                CommandHandlers[attr.Name] = (ICommandHandler)Activator.CreateInstance(type);
            }
        }
    }

    public static void GetFormationStrategies()
    {
        var targetTypes = types
            .Where(t => t.IsAssignableTo(typeof(IFormation)) && !t.IsAbstract && !t.IsInterface);
        foreach (var type in targetTypes)
        {
            if (type.GetCustomAttribute<FormationStrategyAttribute>() is { } strategy)
            {
                Strategies[strategy.Type] = (IFormation)Activator.CreateInstance(type)!;
            }
        }
    }
}
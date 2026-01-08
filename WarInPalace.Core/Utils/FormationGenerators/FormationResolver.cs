using System.Net.Sockets;
using System.Reflection;
using WarInPalace.Core.Enums;
using WarInPalace.Core.Utils.Attributes;

namespace WarInPalace.Core.Utils.FormationGenerators;

public static class FormationResolver
{
    /// <summary>
    /// 策略字典
    /// </summary>
    private static readonly Dictionary<FormationType, IFormation>  _strategies = new();
    
    /// <summary>
    /// 初始化所有策略
    /// </summary>
    static FormationResolver()
    {
        var types = Assembly.GetExecutingAssembly()
                                    .GetTypes()
                                    .Where(t => t.IsAssignableTo(typeof(IFormation)) && !t.IsAbstract && !t.IsInterface);
        foreach (var type in types)
        {
            if (type.GetCustomAttribute<FormationStrategyAttribute>() is { } strategy)
            {
                _strategies[strategy.Type] = (IFormation)Activator.CreateInstance(type)!;
            }
        }
    }
    
    /// <summary>
    /// 获取策略
    /// </summary>
    /// <param name="type">策略类型</param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public static IFormation Get(FormationType type) => _strategies[type] ?? throw new Exception("No such formation type");
}
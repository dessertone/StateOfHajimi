using System.Net.Sockets;
using System.Reflection;
using StateOfHajimi.Core.Enums;
using StateOfHajimi.Core.Utils.Attributes;

namespace StateOfHajimi.Core.Utils.FormationGenerators;

public static class FormationFactory
{
    /// <summary>
    /// 策略字典
    /// </summary>
    private static readonly Dictionary<FormationType, IFormation>  _strategies = new();
    
    
    /// <summary>
    /// 初始化所有策略
    /// </summary>
    static FormationFactory()
    {
        _strategies = AttributeHelper.Strategies;
    }
    
    /// <summary>
    /// 获取策略
    /// </summary>
    /// <param name="type">策略类型</param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public static IFormation Get(FormationType type) => _strategies[type] ?? throw new Exception("No such formation type");
}
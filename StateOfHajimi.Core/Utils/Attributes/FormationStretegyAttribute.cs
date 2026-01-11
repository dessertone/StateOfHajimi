using StateOfHajimi.Core.Enums;

namespace StateOfHajimi.Core.Utils.Attributes;

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class FormationStrategyAttribute: Attribute
{
    public FormationType Type { get; }

    public FormationStrategyAttribute(FormationType type)
    {
        Type = type;
    }
}
using StateOfHajimi.Core.Enums;

namespace StateOfHajimi.Core.Utils.Attributes;

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class BuildUnitTypeAttribute(UnitType unitType) : Attribute
{
    public UnitType UnitType { get;} = unitType;
}
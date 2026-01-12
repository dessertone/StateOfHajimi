using StateOfHajimi.Core.Enums;

namespace StateOfHajimi.Core.Utils.Attributes;

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class BuildUnitTypeAttribute(EntityType entityType) : Attribute
{
    public EntityType EntityType { get;} = entityType;
}
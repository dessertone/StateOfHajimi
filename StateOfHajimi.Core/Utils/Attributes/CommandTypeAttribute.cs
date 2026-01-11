namespace StateOfHajimi.Core.Utils.Attributes;


[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class CommandTypeAttribute:Attribute
{
    public CommandTypeAttribute(string name)
    {
        Name = name;
    }

    public string Name { get; }
    
}
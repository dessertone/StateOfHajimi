using MessagePack;

namespace StateOfHajimi.Engine.MapService;



[MessagePackObject]
public class MapEntityData
{
    [Key(0)]
    public string EntitiyTypeId { get; set; }
    [Key(1)]
    public float X { get; set; }
    [Key(2)]
    public float Y { get; set; }
    [Key(3)]
    public int TeamId { get; set; }
    
}
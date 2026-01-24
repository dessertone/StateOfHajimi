using MessagePack;

namespace StateOfHajimi.Engine.MapService;

[MessagePackObject]
public class MapData
{
    [Key(0)] 
    public string MapName { get; set; } = "untitled";
    [Key(1)]
    public string Version { get; set; } = "1.0.0";
    [Key(2)]
    public int Width { get; set; }
    [Key(3)]
    public int Height { get; set; }
    [Key(4)]
    public int[] TerrainData { get; set; } 
    [Key(5)]
    public List<MapEntityData> Entities { get; set; } = new();
}
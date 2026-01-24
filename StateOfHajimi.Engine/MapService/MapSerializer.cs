using System.Text;
using MessagePack;
using StateOfHajimi.Engine.Data;

namespace StateOfHajimi.Engine.MapService;

public static class MapSerializer
{
    private static readonly byte[] _magicHeader = Encoding.UTF8.GetBytes("StateOfHajimi_MAP_V1");

    public static async Task SaveAsync(string filePath, List<MapEntityData> entities, TileMap map)
    {
        var mapData = new MapData
        { 
            Entities = entities,
            Width = map.Width,
            Height = map.Height,
            TerrainData = new int[map.Width * map.Height],
        };
        for (int y = 0; y < map.Height; y++)
        {
            for (int x = 0; x < map.Width; x++)
            {
                mapData.TerrainData[y * map.Width + x] = (int)map.GetTile(x, y);
            }
        }
        
        var option = MessagePackSerializerOptions.Standard.WithCompression(MessagePackCompression.Lz4BlockArray);
        var bytes =  MessagePackSerializer.Serialize(mapData, option);
        await using var fs = new FileStream(filePath, FileMode.Create);
        await fs.WriteAsync(_magicHeader);
        await fs.WriteAsync(bytes);
    }

    public static async Task<MapData> LoadAsync(string filePath)
    {
        await using var fs = new FileStream(filePath, FileMode.Open);
        var headerBuffer = new byte[_magicHeader.Length];
        var read = await fs.ReadAsync(headerBuffer);
        if (read != _magicHeader.Length || !headerBuffer.SequenceEqual(_magicHeader)) throw new InvalidDataException("Load Failed! Invalid Map Format!");
        
        var options = MessagePackSerializerOptions.Standard.WithCompression(MessagePackCompression.Lz4BlockArray);
        return MessagePackSerializer.Deserialize<MapData>(fs, options);
    }
}
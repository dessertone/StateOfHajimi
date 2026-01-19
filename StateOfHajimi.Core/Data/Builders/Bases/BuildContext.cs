using System.Numerics;

namespace StateOfHajimi.Core.Data.Builders.Bases;

public struct BuildContext(Vector2 position, int teamId, object? extraData = null)
{
    public Vector2 Position = position;
    public int TeamId = teamId;
    public object? ExtraData = extraData;
}
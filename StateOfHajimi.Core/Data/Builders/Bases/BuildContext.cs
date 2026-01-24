using System.Numerics;

namespace StateOfHajimi.Core.Data.Builders.Bases;

public struct BuildContext(Vector2 position, int teamId, params object[] extraData)
{
    public Vector2 Position = position;
    public int TeamId = teamId;
    public object[]? ExtraData = extraData;
}
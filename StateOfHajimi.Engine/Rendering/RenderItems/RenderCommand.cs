using StateOfHajimi.Engine.Enums;

namespace StateOfHajimi.Engine.Rendering.RenderItems;

public record struct RenderCommand(float SortZ,RenderLayer Layer, RenderType Type, int Index):IComparable<RenderCommand>
{
    public int CompareTo(RenderCommand other)
    {
        var layerComparison = Layer.CompareTo(other.Layer);
        if (layerComparison != 0) return layerComparison;
        var sortZComparison = SortZ.CompareTo(other.SortZ);
        if (sortZComparison != 0) return sortZComparison;
        return Index.CompareTo(other.Index);
    }
}
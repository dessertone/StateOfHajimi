namespace StateOfHajimi.Engine.Rendering.RenderItems;

public class RenderContext
{
    private RenderFrame _backgroundRenderFrame = new ();
    private RenderFrame _foregroundRenderFrame = new ();
    private Lock _lock = new ();
    public RenderFrame GetBackgroundRenderFrame()
    {
        _backgroundRenderFrame.Clear();
        return _backgroundRenderFrame;
    }

    public RenderFrame GetForegroundRenderFrame()
    {
        lock (_lock)
        {
            return _foregroundRenderFrame;
        }
    }

    public void SwapFrame()
    {
        lock (_lock)
        {
            (_backgroundRenderFrame, _foregroundRenderFrame) = (_foregroundRenderFrame, _backgroundRenderFrame);
        }
    }
}
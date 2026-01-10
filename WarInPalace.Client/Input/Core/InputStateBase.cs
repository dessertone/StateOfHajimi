using System.Numerics;
using Avalonia.Input;
using HarfBuzzSharp;

namespace WarInPalace.Client.Input.Core;

public abstract class InputStateBase: IInputState
{
    protected InputController Controller { get; private set; }
    protected IGameView GameView => Controller.GameView;
    protected InputBridge Bridge => Controller.InputBridge;
    
    private const float EdgeThreshold = 10f;
    
    
    public virtual void Enter(InputController controller)
    {
        Controller = controller;
    }

    public virtual void Exit()
    {

    }

    /// <summary>
    /// 实现相机边界移动
    /// </summary>
    /// <param name="deltaTime"></param>
    public virtual void Update(float deltaTime)
    {
        var mousePos = GameView.MousePosition;
        if (!GameView.ContainPoint(mousePos)) return;
        var viewport = GameView.ViewportSize;
        
        // 计算移动方向
        var movement = Vector2.Zero;
        if (mousePos.X < viewport.X + EdgeThreshold) movement.X = -1;
        else if (mousePos.X >viewport.X + viewport.Width - EdgeThreshold) movement.X = 1;
        if (mousePos.Y < viewport.Y + EdgeThreshold) movement.Y = -1;
        else if (mousePos.Y > viewport.Y + viewport.Height - EdgeThreshold) movement.Y = 1;

        if (movement != Vector2.Zero)
        {
            GameView.MoveCamera(movement * deltaTime);
        }

    }

    public abstract void OnPointerPressed(PointerPressedEventArgs e);

    public virtual void OnPointerMoved(PointerEventArgs e)
    {
        var (pos, point) = GameView.GetRelativeInfo(e);
        // 更新UI界面鼠标信息
        GameView.MousePosition = pos;
        GameView.Pointer = point;
    }

    public abstract void OnPointerReleased(PointerReleasedEventArgs e);
}
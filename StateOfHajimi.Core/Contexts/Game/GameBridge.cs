using System.Collections.Concurrent;
using System.Numerics;
using Arch.Core;
using StateOfHajimi.Engine.Enums;
using StateOfHajimi.Engine.Input;
using StateOfHajimi.Engine.Input.Commands;

namespace StateOfHajimi.Core.Contexts.Game;



public class GameBridge: IBridge
{
    private Vector2 _mousePosition;
    /// <summary>
    /// 输入指令集合
    /// </summary>
    private readonly ConcurrentQueue<GameCommand> _commands = new();
    /// <summary>
    /// 线程锁
    /// </summary>
    private readonly object o = new();
    
    
    /// <summary>
    /// 当前帧获取的快照
    /// </summary>
    public InputSnapshot CurSnapshot { get; init; } = new(){Commands = []};
    /// <summary>
    /// 更新当前鼠标位置
    /// </summary>
    /// <param name="pos"></param>
    public void UpdateMousePosition(Vector2 pos)
    {
        lock (o)
        {
            _mousePosition = pos;
        }
    }
    /// <summary>
    /// 添加命令
    /// </summary>
    /// <param name="command"></param>
    public void SendCommand(GameCommand command) => _commands.Enqueue(command);
    
    /// <summary>
    /// 获取当前帧的快照
    /// </summary>
    public void CaptureCurrentShot()
    {
        CurSnapshot.Commands.Clear();
        while (_commands.TryDequeue(out var command)) CurSnapshot.Commands.Add(command);
        CurSnapshot.MouseWorldPosition = _mousePosition;
    }
    
    
    public bool IsHovering { get; set; }
    public HoverType CursorHoverType { get; set; }
    public Entity HoveredEntity { get; set; }
}
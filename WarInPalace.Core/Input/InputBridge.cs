using System.Collections.Concurrent;
using System.Numerics;
using WarInPalace.Core.Input.Commands;

namespace WarInPalace.Core.Input;

public class InputBridge
{
    /// <summary>
    /// 当前鼠标位置
    /// </summary>
    private Vector2 _MousePosition;
    
    /// <summary>
    /// 当前帧获取的快照
    /// </summary>
    public InputSnapshot CurSnapshot { get; private set; } = new();
    
    /// <summary>
    /// 输入指令集合
    /// </summary>
    private readonly ConcurrentQueue<GameCommand> _commands = new();
    
    /// <summary>
    /// 线程锁
    /// </summary>
    private readonly object o = new();

    /// <summary>
    /// 更新当前鼠标位置
    /// </summary>
    /// <param name="pos"></param>
    public void UpdateMousePosition(Vector2 pos)
    {
        lock (o)
        {
            _MousePosition = pos;
        }
    }
    
    /// <summary>
    /// 添加命令
    /// </summary>
    /// <param name="command"></param>
    public void AddCommand(GameCommand command) => _commands.Enqueue(command);

    /// <summary>
    /// 获取当前帧的快照
    /// </summary>
    public void CaptureCurrentShot()
    {
        var commands = new List<GameCommand>();
        while (_commands.TryDequeue(out var command)) commands.Add(command);

        CurSnapshot.MouseWorldPosition = _MousePosition;
        CurSnapshot.Commands = commands;
    }
}
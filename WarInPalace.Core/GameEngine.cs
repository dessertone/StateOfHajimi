using System.Runtime.InteropServices.Marshalling;
using Arch.Core;
using WarInPalace.Core.Components;
using WarInPalace.Core.Systems;

namespace WarInPalace.Core;

public class GameEngine:IDisposable
{

    public World GameWorld { get; private set; }
    private SystemGroup _systemGroup = new ();

    public GameEngine()
    {
        GameWorld = World.Create();
        
        // 初始化网格
        _systemGroup.Add(new GridBuildSystem(GameWorld));
        // 框选系统
        _systemGroup.Add(new SelectionSystem(GameWorld));
        // 输入系统
        _systemGroup.Add(new InputSystem(GameWorld));
        // 导航系统
        _systemGroup.Add(new NavigationSystem(GameWorld));
        // 避让系统
        _systemGroup.Add(new AvoidanceSystem(GameWorld));
        // 移动系统
        _systemGroup.Add(new MovementSystem(GameWorld));
        
        _systemGroup.Initialize();
        Test();
    }

    private void Test()
    {
        var speed = 0.2f;
        for (int i = 0; i < 20; ++i)
        {
            GameWorld.Create(
                new Position(40 * i,40 * i),
                new Velocity(0, 0), 
                new MoveSpeed(speed), 
                new Destination{StopDistanceSquared = 256.0f}, 
                BodyCollider.CreateCircle(20 + i, force: speed / 1e4f),
                new IsSelected(),
                new Selectable());
        }
        GameWorld.Create(
            new Position(1000,300),
            new Velocity(0, 0),
            BodyCollider.CreateBox(100, 100, force: speed / 1e4f),
            new IsSelected(),
            new Selectable());
    }
    
    /// <summary>   
    /// 游戏更新逻辑
    /// </summary>
    /// <param name="deltaTime"></param>
    public void Update(float deltaTime)
    {
        // TODO game update logic
        _systemGroup.Update(deltaTime);
        
    }

    public void Dispose()
    {
        GameWorld.Dispose();
    }
}
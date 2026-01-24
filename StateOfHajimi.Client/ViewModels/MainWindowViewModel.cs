using CommunityToolkit.Mvvm.ComponentModel;
using StateOfHajimi.Client.Input;
using StateOfHajimi.Client.Input.States;
using StateOfHajimi.Core;
using StateOfHajimi.Core.Contexts.Game;
using StateOfHajimi.Engine.Input.Core;
using StateOfHajimi.Engine.Rendering.Renderers;
using StateOfHajimi.Engine.Rendering.RenderItems;
using StateOfHajimi.Engine.View;

namespace StateOfHajimi.Client.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    [ObservableProperty]
    private SkiaWorldRenderer _renderer;
    
    [ObservableProperty]
    private GameController _gameController;

    [ObservableProperty]
    private IGameContext _gameContext;

    public MainWindowViewModel()
    {
        _gameContext = new GameContext();
        _renderer = new SkiaWorldRenderer();
        _gameController = new GameController();
    }
}
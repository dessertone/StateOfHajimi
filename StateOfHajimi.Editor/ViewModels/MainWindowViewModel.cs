using CommunityToolkit.Mvvm.ComponentModel;
using StateOfHajimi.Editor.Input;
using StateOfHajimi.Engine.Input;
using StateOfHajimi.Engine.Input.Core;
using StateOfHajimi.Engine.Rendering;
using StateOfHajimi.Engine.Rendering.Renderers;
using StateOfHajimi.Engine.Rendering.RenderItems;
using StateOfHajimi.Engine.View;

namespace StateOfHajimi.Editor.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    [ObservableProperty]
    private IRenderer _renderer = new SkiaWorldRenderer();
    
    [ObservableProperty]
    private IController _editorController = new EditorController();

    [ObservableProperty]
    private IGameContext _gameContext = new EditorContext();
}
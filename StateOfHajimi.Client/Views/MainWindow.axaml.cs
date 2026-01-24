using Avalonia.Controls;
using StateOfHajimi.Client.ViewModels;

namespace StateOfHajimi.Client.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainWindowViewModel();
    }
}
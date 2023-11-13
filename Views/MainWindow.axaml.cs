using Avalonia.Controls;
using Wrench.ViewModels;

namespace Wrench.Views;

public partial class MainWindow : Window
{
    public MainWindow(ViewModelBase viewModel)
    {
        DataContext = viewModel;
        InitializeComponent();
    }
}
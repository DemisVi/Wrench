using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Wrench.ViewModels;

namespace Wrench.Views;

public partial class ControlView : UserControl
{
    public ControlView()
    {
        DataContext = new ControlViewModel();
        InitializeComponent();
    }
}
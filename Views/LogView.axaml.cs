using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using ReactiveUI;

namespace Wrench.Views;

public partial class LogView : UserControl
{
    public readonly ScrollViewer? logScrollViewer;

    public bool? Autoscroll { get => this.FindControl<CheckBox>("autoscroll")!.IsChecked; }
    public LogView()
    {
        InitializeComponent();

        logScrollViewer = this.FindControl<ScrollViewer>("LogScrollViewer");
        if (logScrollViewer is not null)
            logScrollViewer.ScrollChanged += (_, _) =>
            {
                if (Autoscroll is true)
                    Dispatcher.UIThread.Invoke(logScrollViewer.ScrollToEnd);
            };
    }
}
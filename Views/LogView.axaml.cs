using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using ReactiveUI;

namespace Wrench.Views;

public partial class LogView : UserControl
{
    public readonly ScrollViewer? logViewScrollViewer;

    public bool? Autoscroll { get => this.FindControl<CheckBox>("autoscroll")!.IsChecked; }
    public LogView()
    {
        InitializeComponent();

        logViewScrollViewer = this.FindControl<ScrollViewer>("logScrollViewer");
        if (logViewScrollViewer is not null)
            logViewScrollViewer.PropertyChanged += (_, _) =>
            {
                if (Autoscroll is true)
                    Dispatcher.UIThread.Invoke(logViewScrollViewer.ScrollToEnd);
            };
    }
}
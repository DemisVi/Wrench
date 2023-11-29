using System;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using Avalonia.Media;
using DynamicData;
using ReactiveUI;
using Wrench.ViewModels;

namespace Wrench.ViewModels;

public class MainViewModel : ViewModelBase
{
    public StatusViewModel StatusVM { get; set; } = new() { IndicatorColor = (IBrush)Brush.Parse("pink") };
    public LogViewModel LogVM { get; set; } = new();
    public ControlViewModel ControlVM { get; set; } = new();

    public MainViewModel()
    {
        LogVM.Log.AddRange(Enumerable.Range(111, 22).Select(x => x.ToString("X2")));
    }

    public void ShowPackageSelector()
    {
        // Observable.Subscribe(ControlVM.)
    }
}

using System;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using Avalonia.Media;
using DynamicData;
using ReactiveUI;
using Wrench.Models;
using Wrench.ViewModels;

namespace Wrench.ViewModels;

public class MainViewModel : ViewModelBase
{
    private Package? package;

    public ControlViewModel ControlViewModel { get; set; } = new();
    public StatusViewModel StatusViewModel { get; set; } = new()
    {
        Bad = 20,
        Good = 40000,
        ContactUnit = "CanCom150",
        Elapsed = TimeSpan.FromSeconds(500),
        SerialNumber = "asdfasdfasdf1",
        Label = "wtf",
    };
    public Package? Package { get => package; set => this.RaiseAndSetIfChanged(ref package, value); }
}

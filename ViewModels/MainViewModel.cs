using System;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using Avalonia.Media;
using DynamicData;
using Iot.Device.FtCommon;
using ReactiveUI;
using Wrench.Models;
using Wrench.ViewModels;
using Wrench.Views;

namespace Wrench.ViewModels;

public class MainViewModel : ViewModelBase
{
    private Package? package;

    public MainViewModel()
    {
        var fts = Ftx232HDevice.GetFtx232H();
        if (fts is { Count: > 0 })
            StatusViewModel.ContactUnit = fts.First().SerialNumber.TrimEnd('A', 'B');
        
        
    }

    public ControlViewModel ControlViewModel { get; set; } = new();
    public StatusViewModel StatusViewModel { get; set; } = new();
    public Package? Package { get => package; set => this.RaiseAndSetIfChanged(ref package, value); }
}

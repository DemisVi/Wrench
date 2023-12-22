using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using Avalonia.Media;
using DynamicData;
using Iot.Device.FtCommon;
using ReactiveUI;
using Wrench.Models;
using Wrench.Services;
using Wrench.ViewModels;
using Wrench.Views;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Avalonia.Threading;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Threading;

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
    public LogViewModel LogViewModel { get; set; } = new();
    public Package? Package { get => package; set => this.RaiseAndSetIfChanged(ref package, value); }

    public void FireTool()
    {
        var flasher = new Flasher();
        var commands = new List<FlasherCommand>
        {
            new(flasher.SignalReady),
            new(delegate () { Thread.Sleep(1000); }),
            new(flasher.LockCU),
            new(delegate () { Thread.Sleep(2000); }),
            new(flasher.TurnModemPowerOn),
            new(flasher.AwaitDeviceAttach),
            new(flasher.TurnModemPowerOff),
            new(delegate () { Thread.Sleep(1000); }),
            new(flasher.UnlockCU)
        };
        var sequence = new CommandSequence(commands);

        sequence.Run();
    }
}

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
using Wrench.DataTypes;
using System.IO;
using System.Text.Json;

namespace Wrench.ViewModels;

public class MainViewModel : ViewModelBase
{
    private Package? package;
    private CancellationTokenSource? cts;

    public MainViewModel()
    {
        var fts = Ftx232HDevice.GetFtx232H();
        if (fts is { Count: > 0 })
            StatusViewModel.ContactUnit = fts.First().SerialNumber.TrimEnd('A', 'B');
#if DEBUG
        FireTool = ReactiveCommand.Create(PerformFireTool);
#else
        FireTool = ReactiveCommand.Create(PerformFireTool, this.WhenAnyValue(x => x.Package, x => (Package?)x is not null));
#endif
    }

    public ControlViewModel ControlViewModel { get; set; } = new();
    public StatusViewModel StatusViewModel { get; set; } = new();
    public LogViewModel LogViewModel { get; set; } = new();
    public ReactiveCommand<Unit, Unit> FireTool { get; set; }
    public Package? Package { get => package; set => this.RaiseAndSetIfChanged(ref package, value); }

    public void PerformFireTool()
    {
        cts = new CancellationTokenSource();
        var flasher = new Flasher();

        var commands = new List<FlasherCommand>
        {
            new(flasher.TurnModemPowerOff),
            new(flasher.TurnModemPowerOn),
            new(flasher.AwaitDeviceAttach),
            new(flasher.AwaitDeviceStart),
            FlasherCommand.Create(flasher.Sleep, 8),
            new(flasher.TurnOnADBInterface) { CommandNote = "ADB ON" },
            FlasherCommand.Create(flasher.Sleep, 8),
            new(flasher.TurnOffADBInterface) { CommandNote = "ADB OFF" },
            FlasherCommand.Create(flasher.Sleep, 8),
            new(flasher.TurnOnADBInterface) { CommandNote = "ADB ON" },
            FlasherCommand.Create(flasher.Sleep, 8),
            new(flasher.TurnOffADBInterface) { CommandNote = "ADB OFF" },
            FlasherCommand.Create(flasher.Sleep, 8),
        };

        var sequence = new CommandSequence(commands: commands,
                                           log: Log);


        Task.Factory.StartNew(() =>
        {
            sequence.Run(cts.Token);
        }/* , cts.Token */);

        void Log(string str) => Dispatcher.UIThread.Invoke(() => LogViewModel.Log.Add(str));
    }

    public void PerformCancel()
    {
        cts?.Cancel();
    }
}

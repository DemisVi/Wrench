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
using System.Threading;
using Wrench.DataTypes;
using System.IO;
using System.Text.Json;

namespace Wrench.ViewModels;

public class MainViewModel : ViewModelBase
{
    private Package? package;
    private CancellationTokenSource? cts;
    private bool isFlasherRunning;

    public bool IsFlasherRunning { get => isFlasherRunning; private set => this.RaiseAndSetIfChanged(ref isFlasherRunning, value); }
    public ControlViewModel ControlViewModel { get; set; } = new();
    public StatusViewModel StatusViewModel { get; set; } = new();
    public LogViewModel LogViewModel { get; set; } = new();
    public ReactiveCommand<Unit, Unit> FireTool { get; set; }
    public Package? Package { get => package; set => this.RaiseAndSetIfChanged(ref package, value); }



    public MainViewModel()
    {
        var fts = Ftx232HDevice.GetFtx232H();
        if (fts is { Count: > 0 })
            StatusViewModel.ContactUnit = fts.First().SerialNumber.TrimEnd('A', 'B');
#if DEBUG
        FireTool = ReactiveCommand.Create(PerformFireTool);
#else
        FireTool = ReactiveCommand.Create(PerformFireTool, this.WhenAnyValue(x => x.Package,
                                                                             y => y.IsFlasherRunning,
                                                                             (x, y) => (Package?)x is not null && y is false));
#endif
    }

    public void PerformFireTool()
    {
        IsFlasherRunning = true;
        cts = new CancellationTokenSource();
        using var flasher = new Flasher();

        var initCommands = new List<FlasherCommand>
        {
            new(flasher.TurnModemPowerOff),
            new(flasher.TurnModemPowerOn),
            new(flasher.AwaitDeviceAttach),
            new(flasher.CheckDeviceResponding),
        };

        var fastbootCommands = new List<FlasherCommand>
        {
            FlasherCommand.Create(flasher.Sleep, 4),
            new(flasher.ExecuteFastbootBatch),
        };

        var initSequence = new CommandSequence(commands: initCommands,
                                               log: Log);
        var fastbootSequence = new CommandSequence(commands: fastbootCommands,
                                                   canExecute: new(() => flasher.CheckADBDevice().ResponseType == ResponseType.OK),
                                                   log: Log);

        Task.Factory.StartNew(() =>
        {
            // initSequence.Run(cts.Token);
            fastbootSequence.Run(cts.Token);

            Dispatcher.UIThread.Invoke(() => IsFlasherRunning = false);

        }, cts.Token);

        void Log(string str) => Dispatcher.UIThread.Invoke(() => LogViewModel.Log.Add(str));
    }

    public void PerformCancel()
    {
        cts?.Cancel();
    }
}

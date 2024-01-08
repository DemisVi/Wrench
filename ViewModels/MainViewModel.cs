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
            new(() => flasher.AwaitCUSignal(cts.Token)) { CommandNote = "Ожидание сигнала готовности КУ" },
            new(flasher.AwaitDeviceAttach) { CommandNote = "Ожидание подключения устройства" },
            new(flasher.CheckADBDevice) { CommandNote = "Проверка типа устройства" },
        };
        var sequence = new CommandSequence(commands: commands,
                                           log: Log);

        File.WriteAllText("./comms.json", JsonSerializer.Serialize(commands));

        Task.Factory.StartNew(() =>
        {
            sequence.Run(cts.Token);
            flasher.Dispose();
        }, cts.Token);

        void Log(string str) => Dispatcher.UIThread.Invoke(() => LogViewModel.Log.Add(str));
    }

    public void PerformCancel()
    {
        cts?.Cancel();
    }
}

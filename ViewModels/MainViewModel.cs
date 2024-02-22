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
        var opRes = new FlasherResponse();

        Task.Factory.StartNew(() =>
        {
            using var flasher = new Flasher();

            Log("START:");
            Log("-= 4 seconds pause for FTDI =-");
            ExecuteWithLogging(() => flasher.Sleep(4));
            ExecuteWithLogging(() => new(ResponseType.Info) { ResponseMessage = "Start adb server..."});
            ExecuteWithLogging(() => flasher.Adb("start-server", 6));

            // while (!cts.IsCancellationRequested)
            // {
            //     if (ExecuteWithLogging(() => flasher.AwaitCUReady(cts.Token)) is not { ResponseType: ResponseType.OK }
            //         || ExecuteWithLogging(() => new(ResponseType.Info) { ResponseMessage = "Lock CU..." }) is null
            //         || ExecuteWithLogging(() => flasher.LockCU()) is not { ResponseType: ResponseType.OK }
            //         || ExecuteWithLogging(() => new(ResponseType.Info) { ResponseMessage = "Power off device..." }) is null
            //         || ExecuteWithLogging(() => flasher.TurnModemPowerOff()) is not { ResponseType: ResponseType.OK }
            //         || ExecuteWithLogging(() => flasher.Sleep(1)) is not { ResponseType: ResponseType.OK }
            //         || ExecuteWithLogging(() => new(ResponseType.Info) { ResponseMessage = "Power on device..." }) is null
            //         || ExecuteWithLogging(() => flasher.TurnModemPowerOn()) is not { ResponseType: ResponseType.OK }
            //         || ExecuteWithLogging(() => new(ResponseType.Info) { ResponseMessage = "Waiting for device..." }) is null
            //         || ExecuteWithLogging(() => flasher.AwaitDeviceAttach()) is not { ResponseType: ResponseType.OK }
            //         || ExecuteWithLogging(() => new(ResponseType.Info) { ResponseMessage = "Waiting device boot up..." }) is null
            //         || ExecuteWithLogging(() => flasher.Sleep(16)) is not { ResponseType: ResponseType.OK }
            //         || ExecuteWithLogging(() => new(ResponseType.Info) { ResponseMessage = "Check device response..." }) is null
            //         || ExecuteWithLogging(() => flasher.CheckDeviceResponding()) is not { ResponseType: ResponseType.OK })
            //     {
            //         Log("FAIL!");
            //         continue;
            //     }
            //     else if (ExecuteWithLogging(() => new(ResponseType.Info) { ResponseMessage = "Check device is ADB..." }) is null
            //              || ExecuteWithLogging(() => flasher.CheckADBDevice()) is not { ResponseType: ResponseType.OK }
            //              || ExecuteWithLogging(() => new(ResponseType.Info) { ResponseMessage = "Turning on ADB interface..." }) is null
            //              || ExecuteWithLogging(() => flasher.TurnOnADBInterface()) is not { ResponseType: ResponseType.OK }
            //              || ExecuteWithLogging(() => flasher.Sleep(4)) is not { ResponseType: ResponseType.OK }
            //              || ExecuteWithLogging(() => new(ResponseType.Info) { ResponseMessage = "Check if ADB is connected..." }) is null
            //              || ExecuteWithLogging(() => flasher.CheckADBDevice()) is not { ResponseType: ResponseType.OK }
            //              || ExecuteWithLogging(() => new(ResponseType.Info) { ResponseMessage = "Running fastboot batch..." }) is null
            //              || ExecuteWithLogging(() => flasher.ExecuteFastbootBatch()) is not { ResponseType: ResponseType.OK }
            //              )
            //     {
            //         Log("FAIL!");
            //         continue;
            //     }

            //     Log("Some success ;]");
            // }

            Dispatcher.UIThread.Invoke(() => IsFlasherRunning = false);
        },
            cts.Token);

        FlasherResponse ExecuteWithLogging(Func<FlasherResponse> func)
        {
            opRes = func();
            Report();
            return opRes;
        }

        void Report() => Log(string.Join(": ", opRes.ResponseType, opRes.ResponseMessage));
        void Log(string str) => Dispatcher.UIThread.Invoke(() => LogViewModel.Log.Add(str));
    }

    public void PerformCancel()
    {
        cts?.Cancel();
    }
}

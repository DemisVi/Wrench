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
using System.Net.Http;
using Avalonia;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Wrench.Extensions;

namespace Wrench.ViewModels;

public class MainViewModel : ViewModelBase
{
    private Package? package;
    private CancellationTokenSource? cts;
    private bool isFlasherRunning;
    private const string baseFirmwarePrefix = "./base";

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
        this.WhenAnyValue(x => x.Package).Subscribe(x => StatusViewModel.SerialNumber = ReadSerial(x));
    }

    private string ReadSerial(Package? x)
    {
        if (x is null) return string.Empty;
        try
        {
            var fac = new FactoryCFG(Path.GetDirectoryName(x.PackagePath));
            fac.ReadFactory();
            var base34serial = fac.SerialNumber.ToString();
            return $"{base34serial} ({base34serial.ToInt32()})";
        }
        catch (FileNotFoundException)
        {
            return string.Empty;
        }
    }

    public void PerformFireTool()
    {
        IsFlasherRunning = true;
        cts = new CancellationTokenSource();
        var opRes = new FlasherResponse();

        Task.Factory.StartNew(() =>
        {
            using Flasher flasher = new();
            flasher.Package = Package;

            Log("START:");
            Log("-= 4 seconds pause for FTDI =-");
            ExecuteWithLogging(() => flasher.Sleep(4));
            ExecuteWithLogging(() => new(ResponseType.Info) { ResponseMessage = "Start adb server..." });
            ExecuteWithLogging(() => flasher.Adb("start-server", 6));

            while (!cts.IsCancellationRequested)
            {
                ExecuteWithLogging(() => new(ResponseType.Info) { ResponseMessage = FlasherMessages.FlashBaseFirmware });

                if (ExecuteWithLogging(() => new(ResponseType.Info) { ResponseMessage = "Waiting for CU signal..." }) is null
                    || ExecuteWithLogging(() => flasher.SignalReady()) is not { ResponseType: ResponseType.OK }
                    || ExecuteWithLogging(() => flasher.AwaitCUReady(cts.Token)) is not { ResponseType: ResponseType.OK }
                    || ExecuteWithLogging(() => new(ResponseType.Info) { ResponseMessage = "Lock CU..." }) is null
                    || ExecuteWithLogging(() => flasher.LockCU()) is not { ResponseType: ResponseType.OK }
                    || ExecuteWithLogging(() => new(ResponseType.Info) { ResponseMessage = "Power off device..." }) is null
                    || ExecuteWithLogging(() => flasher.TurnModemPowerOff()) is not { ResponseType: ResponseType.OK }
                    || ExecuteWithLogging(() => flasher.Sleep(2)) is not { ResponseType: ResponseType.OK }
                    || ExecuteWithLogging(() => new(ResponseType.Info) { ResponseMessage = "Power on device..." }) is null
                    || ExecuteWithLogging(() => flasher.TurnModemPowerOn()) is not { ResponseType: ResponseType.OK }
                    || ExecuteWithLogging(() => new(ResponseType.Info) { ResponseMessage = "Waiting for device..." }) is null
                    || ExecuteWithLogging(() => flasher.AwaitDeviceAttach()) is not { ResponseType: ResponseType.OK }
                    || ExecuteWithLogging(() => new(ResponseType.Info) { ResponseMessage = "Waiting device boot up..." }) is null
                    || ExecuteWithLogging(() => flasher.Sleep(16)) is not { ResponseType: ResponseType.OK }
                    || ExecuteWithLogging(() => new(ResponseType.Info) { ResponseMessage = "Check device response..." }) is null
                    || ExecuteWithLogging(() => flasher.CheckDeviceResponding()) is not { ResponseType: ResponseType.OK })
                {
                    FailState();
                    continue;
                }
                else if (ExecuteWithLogging(() => new(ResponseType.Info) { ResponseMessage = "Check device is ADB..." }) is null
                         || ExecuteWithLogging(() => flasher.CheckADBDevice()) is not { ResponseType: ResponseType.OK })
                {
                    if (ExecuteWithLogging(() => new(ResponseType.Info) { ResponseMessage = "Turning on ADB interface..." }) is null
                        || ExecuteWithLogging(() => flasher.TurnOnADBInterface()) is not { ResponseType: ResponseType.OK }
                        || ExecuteWithLogging(() => flasher.Sleep(6)) is not { ResponseType: ResponseType.OK }
                        || ExecuteWithLogging(() => new(ResponseType.Info) { ResponseMessage = "Check if ADB is connected..." }) is null
                        || ExecuteWithLogging(() => flasher.CheckADBDevice()) is not { ResponseType: ResponseType.OK })
                    {
                        FailState();
                        continue;
                    }
                }
                ExecuteWithLogging(() => new(ResponseType.Info) { ResponseMessage = "Running android batch..." });
                if (ExecuteWithLogging(() => flasher.Adb(flasher.AdbRebootBootloaderCommand, 1)) is not { ResponseType: ResponseType.OK })
                {
                    FailState();
                    continue;
                }
                ExecuteWithLogging(() => flasher.Sleep(2));
                // if (ExecuteWithLogging(() => ExecuteFastboot()) is not { ResponseType: ResponseType.OK })
                //     continue;
                ExecuteWithLogging(() => flasher.Sleep(1));
                if (ExecuteWithLogging(() => flasher.Fastboot(flasher.FastbootRebootCommand, 1)) is not { ResponseType: ResponseType.OK })
                {
                    FailState();
                    continue;
                }
                if (Package is not null and { DeviceType: DeviceType.SimComSimple })
                {
                    if (ExecuteWithLogging(() => new(ResponseType.Info) { ResponseMessage = "Waiting for device..." }) is null
                        || ExecuteWithLogging(() => flasher.AwaitDeviceAttach()) is not { ResponseType: ResponseType.OK }
                        || ExecuteWithLogging(() => new(ResponseType.Info) { ResponseMessage = "Check device is ADB..." }) is null
                        || ExecuteWithLogging(() => flasher.CheckADBDevice()) is not { ResponseType: ResponseType.OK })
                    {
                        ExecuteWithLogging(() => new(ResponseType.Info) { ResponseMessage = FlasherMessages.EnableADB });

                        if (ExecuteWithLogging(() => new(ResponseType.Info) { ResponseMessage = "Waiting device boot up..." }) is null
                            || ExecuteWithLogging(() => flasher.Sleep(16)) is not { ResponseType: ResponseType.OK }
                            || ExecuteWithLogging(() => new(ResponseType.Info) { ResponseMessage = "Check device response..." }) is null
                            || ExecuteWithLogging(() => flasher.CheckDeviceResponding()) is not { ResponseType: ResponseType.OK }
                            || ExecuteWithLogging(() => new(ResponseType.Info) { ResponseMessage = "Turning on ADB interface..." }) is null
                            || ExecuteWithLogging(() => flasher.TurnOnADBInterface()) is not { ResponseType: ResponseType.OK }
                            || ExecuteWithLogging(() => flasher.Sleep(6)) is not { ResponseType: ResponseType.OK }
                            || ExecuteWithLogging(() => new(ResponseType.Info) { ResponseMessage = "Check if ADB is connected..." }) is null
                            || ExecuteWithLogging(() => flasher.CheckADBDevice()) is not { ResponseType: ResponseType.OK })
                        {
                            FailState();
                            continue;
                        }
                    }
                    ExecuteWithLogging(() => new(ResponseType.Info) { ResponseMessage = FlasherMessages.UploadFactory });

                    if (ExecuteWithLogging(() => new(ResponseType.Info) { ResponseMessage = "Upload 'factory.cfg'..." }) is null
                        || ExecuteWithLogging(() => flasher.UploadFactoryCFG()) is not { ResponseType: ResponseType.OK }
                        || ExecuteWithLogging(() => new(ResponseType.Info) { ResponseMessage = "Update 'factory.cfg'..." }) is null
                        || ExecuteWithLogging(() => flasher.UpdateCfgSN()) is not { ResponseType: ResponseType.OK }
                        || ExecuteWithLogging(() => new(ResponseType.Info) { ResponseMessage = "Turning off ADB interface..." }) is null
                        || ExecuteWithLogging(() => flasher.TurnOffADBInterface()) is not { ResponseType: ResponseType.OK })
                    {
                        FailState();
                        continue;
                    }
                }
                else if (Package is not null and { DeviceType: DeviceType.SimComRetro }
                         || Package is not null and { DeviceType: DeviceType.SimComFull })
                {
                    if (ExecuteWithLogging(() => new(ResponseType.Info) { ResponseMessage = "Waiting for device..." }) is null
                        || ExecuteWithLogging(() => flasher.AwaitDeviceAttach()) is not { ResponseType: ResponseType.OK }
                        || ExecuteWithLogging(() => new(ResponseType.Info) { ResponseMessage = "Check device is ADB..." }) is null
                        || ExecuteWithLogging(() => flasher.CheckADBDevice()) is { ResponseType: ResponseType.OK })
                    {
                        if (ExecuteWithLogging(() => new(ResponseType.Info) { ResponseMessage = "Waiting device boot up..." }) is null
                            || ExecuteWithLogging(() => flasher.Sleep(16)) is not { ResponseType: ResponseType.OK }
                            || ExecuteWithLogging(() => new(ResponseType.Info) { ResponseMessage = "Check device response..." }) is null
                            || ExecuteWithLogging(() => flasher.CheckDeviceResponding()) is not { ResponseType: ResponseType.OK }
                            || ExecuteWithLogging(() => new(ResponseType.Info) { ResponseMessage = "Turning off ADB interface..." }) is null
                            || ExecuteWithLogging(() => flasher.TurnOffADBInterface()) is not { ResponseType: ResponseType.OK })
                        {
                            FailState();
                            continue;
                        }
                    }
                }

                SuccessState();
            }

            void SuccessState()
            {
                ExecuteWithLogging(() => flasher.UnlockCU());
                StatusViewModel.SerialNumber = ReadSerial(Package);
                Log("Some success ;]");
                ExecuteWithLogging(() => new(ResponseType.Info) { ResponseMessage = "Waiting Contact Unit release..." });
                ExecuteWithLogging(() => flasher.SignalDone());
                ExecuteWithLogging(() => flasher.AwaitCURelease(cts.Token));
            }

            void FailState()
            {
                ExecuteWithLogging(() => flasher.UnlockCU());
                Log("FAIL!");
                ExecuteWithLogging(() => new(ResponseType.Info) { ResponseMessage = "Waiting Contact Unit release..." });
                ExecuteWithLogging(() => flasher.SignalFail());
                ExecuteWithLogging(() => flasher.AwaitCURelease(cts!.Token));

            }

            FlasherResponse ExecuteFastboot()
            {
                foreach (var command in flasher.FastbootCommandSequence)
                {
                    if (ExecuteWithLogging(() => flasher.Fastboot(command.Key, command.Value)) is not { ResponseType: ResponseType.OK })
                        return new(ResponseType.Fail) { ResponseMessage = command.Key };
                }
                return new(ResponseType.OK) { ResponseMessage = nameof(flasher.FastbootCommandSequence) };
            }

            Dispatcher.UIThread.Invoke(() => IsFlasherRunning = false);
        },
            cts.Token);

        FlasherResponse ExecuteWithLogging(Func<FlasherResponse> func)
        {
            var start = DateTime.Now;
            opRes = func();
            Report(DateTime.Now - start);
            return opRes;
        }

        void Report(TimeSpan span) => Log(string.Join(": ", DateTime.Now.ToString("T"), $"[{span:ss':'fff}]", opRes.ResponseType, opRes.ResponseMessage));
        void Log(string str) => Dispatcher.UIThread.Invoke(() => LogViewModel.Log.Add(str));
    }

    public void PerformCancel()
    {
        cts?.Cancel();
    }
}

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Wrench.DataTypes;
using Wrench.Models;
using Wrench.Extensions;

using Timer = System.Timers.Timer;

namespace Wrench.Services;

public class FlasherController
{
    private const string baseFirmwarePrefix = "./base";
    private const int BootUpTimeout = 15,
        ADBSwitchTimeout = 10,
        BootloaderRebootTimeout = 40;

    public bool IsFlasherRunning { get; private set; }
    public CancellationTokenSource? Cts { get; set; }
    public event FlasherControllerEventHandler? EventOccurred;
    public Package? Package { get; set; }

    public void RunFlasher()
    {
        Cts = new CancellationTokenSource();
        var opRes = new FlasherResponse();
        var timer = new Timer(TimeSpan.FromSeconds(1));
        timer.Elapsed += (_, _) => EventOccurred?.Invoke(this, new(FlasherControllerEventType.ProgressTimerElapsed));

        Task.Factory.StartNew(() =>
        {
            using Flasher flasher = new();
            flasher.Package = Package;

            EventOccurred?.Invoke(this, new(FlasherControllerEventType.FlasherStateChanged) { Payload = false });

            Log("START:");
            Log("-= 4 seconds pause for FTDI =-");
            ExecuteWithLogging(() => flasher.Sleep(4));
            ExecuteWithLogging(() => new(ResponseType.Info) { ResponseMessage = "Start adb server..." });
            ExecuteWithLogging(() => flasher.Adb("start-server", 6));

            while (!Cts.IsCancellationRequested)
            {
                ExecuteWithLogging(() => new(ResponseType.Info) { ResponseMessage = FlasherMessages.FlashBaseFirmware });

                if (ExecuteWithLogging(() => new(ResponseType.Info) { ResponseMessage = "Waiting for CU signal..." }) is null
                    || ExecuteWithLogging(() => flasher.SignalReady()) is not { ResponseType: ResponseType.OK }
                    || ExecuteWithLogging(() => SignalReady()) is null
                    || ExecuteWithLogging(() => flasher.AwaitCUReady(Cts.Token)) is not { ResponseType: ResponseType.OK }
                    || ExecuteWithLogging(() => StartStopwatch()) is null
                    || ExecuteWithLogging(() => new(ResponseType.Info) { ResponseMessage = "Lock CU..." }) is null
                    || ExecuteWithLogging(() => flasher.SignalBusy()) is not { ResponseType: ResponseType.OK }
                    || ExecuteWithLogging(() => SignalBusy()) is null
                    || ExecuteWithLogging(() => flasher.LockCU()) is not { ResponseType: ResponseType.OK }
                    || ExecuteWithLogging(() => flasher.Sleep(1)) is not { ResponseType: ResponseType.OK }
                    || ExecuteWithLogging(() => new(ResponseType.Info) { ResponseMessage = "Power on device..." }) is null
                    || ExecuteWithLogging(() => flasher.TurnModemPowerOn()) is not { ResponseType: ResponseType.OK }
                    || ExecuteWithLogging(() => new(ResponseType.Info) { ResponseMessage = "Waiting for device..." }) is null
                    || ExecuteWithLogging(() => flasher.AwaitDeviceAttach()) is not { ResponseType: ResponseType.OK }
                    || ExecuteWithLogging(() => new(ResponseType.Info) { ResponseMessage = "Waiting device boot up..." }) is null
                    || ExecuteWithLogging(() => flasher.Sleep(BootUpTimeout)) is not { ResponseType: ResponseType.OK }
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
                        || ExecuteWithLogging(() => flasher.Sleep(ADBSwitchTimeout)) is not { ResponseType: ResponseType.OK }
                        || ExecuteWithLogging(() => new(ResponseType.Info) { ResponseMessage = "Check if ADB is connected..." }) is null
                        || ExecuteWithLogging(() => flasher.CheckADBDevice()) is not { ResponseType: ResponseType.OK })
                    {
                        FailState();
                        continue;
                    }
                }
                ExecuteWithLogging(() => flasher.Sleep(2));
                ExecuteWithLogging(() => new(ResponseType.Info) { ResponseMessage = "Running android batch..." });
                if (ExecuteWithLogging(() => flasher.Adb(flasher.AdbRebootBootloaderCommand, BootloaderRebootTimeout)) is not { ResponseType: ResponseType.OK })
                {
                    FailState();
                    continue;
                }
                ExecuteWithLogging(() => flasher.Sleep(2));
                if (ExecuteWithLogging(() => ExecuteFastboot()) is not { ResponseType: ResponseType.OK })
                {
                    FailState();
                    continue;
                }
                ExecuteWithLogging(() => flasher.Sleep(2));
                if (ExecuteWithLogging(() => flasher.Fastboot(flasher.FastbootRebootCommand, 2)) is not { ResponseType: ResponseType.OK })
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
                            || ExecuteWithLogging(() => flasher.Sleep(BootUpTimeout)) is not { ResponseType: ResponseType.OK }
                            || ExecuteWithLogging(() => new(ResponseType.Info) { ResponseMessage = "Check device response..." }) is null
                            || ExecuteWithLogging(() => flasher.CheckDeviceResponding()) is not { ResponseType: ResponseType.OK }
                            || ExecuteWithLogging(() => new(ResponseType.Info) { ResponseMessage = "Turning on ADB interface..." }) is null
                            || ExecuteWithLogging(() => flasher.TurnOnADBInterface()) is not { ResponseType: ResponseType.OK }
                            || ExecuteWithLogging(() => flasher.Sleep(ADBSwitchTimeout)) is not { ResponseType: ResponseType.OK }
                            || ExecuteWithLogging(() => new(ResponseType.Info) { ResponseMessage = "Check if ADB is connected..." }) is null
                            || ExecuteWithLogging(() => flasher.CheckADBDevice()) is not { ResponseType: ResponseType.OK })
                        {
                            FailState();
                            continue;
                        }
                    }
                    ExecuteWithLogging(() => new(ResponseType.Info) { ResponseMessage = FlasherMessages.UploadFactory });

                    if (ExecuteWithLogging(() => new(ResponseType.Info) { ResponseMessage = "Upload 'factory.cfg'..." }) is null
                        || ExecuteWithLogging(() => flasher.Sleep(2)) is null
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
                            || ExecuteWithLogging(() => flasher.Sleep(BootUpTimeout)) is not { ResponseType: ResponseType.OK }
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

            FlasherResponse StartStopwatch()
            {
                try
                {
                    timer.Start();
                    return new FlasherResponse(ResponseType.OK) { ResponseMessage = FlasherMessages.TimerStart, };
                }
                catch (Exception ex)
                {
                    return new FlasherResponse(ex);
                }
            }

            FlasherResponse StopStopwatch()
            {
                try
                {
                    timer!.Stop();
                    return new FlasherResponse(ResponseType.OK) { ResponseMessage = FlasherMessages.TimerStop, };
                }
                catch (Exception ex)
                {
                    return new FlasherResponse(ex);
                }
            }

            FlasherResponse ResetStopwatch()
            {
                try
                {
                    EventOccurred?.Invoke(this, new(FlasherControllerEventType.ProgressTimerReset));
                    return new FlasherResponse(ResponseType.OK) { ResponseMessage = FlasherMessages.TimerReset, };
                }
                catch (Exception ex)
                {
                    return new FlasherResponse(ex);
                }
            }

            void SuccessState()
            {
                ExecuteWithLogging(() => flasher.TurnModemPowerOff());
                ExecuteWithLogging(() => flasher.UnlockCU());
                ExecuteWithLogging(() => StopStopwatch());
                EventOccurred?.Invoke(this, new(FlasherControllerEventType.SuccessState) { Payload = ReadSerial(Package) });
                Log("Some success ;]");
                ExecuteWithLogging(() => new(ResponseType.Info) { ResponseMessage = "Waiting Contact Unit release..." });
                ExecuteWithLogging(() => flasher.SignalDone());
                ExecuteWithLogging(() => flasher.AwaitCURelease(Cts.Token));
                ExecuteWithLogging(() => ResetStopwatch());
            }

            void FailState()
            {
                ExecuteWithLogging(() => flasher.TurnModemPowerOff());
                ExecuteWithLogging(() => flasher.UnlockCU());
                ExecuteWithLogging(() => StopStopwatch());
                EventOccurred?.Invoke(this, new(FlasherControllerEventType.FailState));
                Log("FAIL!");
                ExecuteWithLogging(() => new(ResponseType.Info) { ResponseMessage = "Waiting Contact Unit release..." });
                ExecuteWithLogging(() => flasher.SignalFail());
                ExecuteWithLogging(() => flasher.AwaitCURelease(Cts!.Token));
                ExecuteWithLogging(() => ResetStopwatch());

            }

            FlasherResponse SignalReady()
            {
                EventOccurred?.Invoke(this, new(FlasherControllerEventType.SignalReady));
                return new(ResponseType.OK) { ResponseMessage = "Signal ready" };
            }

            FlasherResponse SignalBusy()
            {
                EventOccurred?.Invoke(this, new(FlasherControllerEventType.SignalBusy));
                return new(ResponseType.OK) { ResponseMessage = "Signal busy" };
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

            EventOccurred?.Invoke(this, new(FlasherControllerEventType.FlasherStateChanged) { Payload = false }); // Dispatcher.UIThread.Invoke(() => IsFlasherRunning = false);
        },
            Cts.Token);

        FlasherResponse ExecuteWithLogging(Func<FlasherResponse> func)
        {
            var start = DateTime.Now;
            opRes = func();
            Report(DateTime.Now - start);
            return opRes;
        }

        void Report(TimeSpan span) => Log(string.Join(": ", DateTime.Now.ToString("T"), $"[{span:ss':'fff}]", opRes.ResponseType, opRes.ResponseMessage));
        void Log(string str) => EventOccurred?.Invoke(this, new(FlasherControllerEventType.LogMessage) { Payload = str });/* Dispatcher.UIThread.Invoke(() => LogViewModel.Log.Add(str)); */
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

}

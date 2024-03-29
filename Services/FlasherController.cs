using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Wrench.DataTypes;
using Wrench.Models;
using Wrench.Extensions;

using Timer = System.Timers.Timer;
using Microsoft.VisualBasic;
using System.Linq;

namespace Wrench.Services;

public class FlasherController : IDisposable
{
    private const string baseFirmwarePrefix = "./base";
    private const int BootUpTimeout = 15,
        ADBSwitchTimeout = 10,
        BootloaderRebootTimeout = 40;
    private const int SWDConsoleTimeout = 60;
    private bool disposedValue;
    private Timer timer = new(TimeSpan.FromSeconds(1));

    public FlasherController()
    {
        timer.Elapsed += (_, _) => EventOccurred?.Invoke(this, new(FlasherControllerEventType.ProgressTimerElapsed));
    }

    public bool IsFlasherRunning { get; private set; }
    public CancellationTokenSource? Cts { get; set; }
    public event FlasherControllerEventHandler? EventOccurred;
    public Package? Package { get; set; }
    public IFlasher? Flasher { get; set; }

    public void RunFlasher()
    {
        if (Cts is null or { IsCancellationRequested: true }) throw new NullReferenceException($"{nameof(Cts)} is null or Cancellation requested");

        var opRes = new FlasherResponse();

        if (Package is null)
        {
            Log($"{nameof(Package)} is null. Terminating.");
            return;
        }

        if (Flasher is null)
        {
            Log($"{nameof(Flasher)} is null. Terminating");
            return;
        }

        Task.Factory.StartNew(Flasher switch
        {
            SimComFlasher => delegate ()
            {
                Flasher.Package = Package;

                EventOccurred?.Invoke(this, new(FlasherControllerEventType.FlasherStateChanged) { Payload = false });

                Log("START:");
                Log("-= 4 seconds pause for FTDI =-");
                ExecuteWithLogging(() => Flasher.Sleep(4));
                ExecuteWithLogging(() => new(ResponseType.Info) { ResponseMessage = "Start adb server..." });
                ExecuteWithLogging(() => Flasher.Adb("start-server", 6));

                if (Cts.IsCancellationRequested)
                    Log($"Cancellation requested. {nameof(Cts)} Token is {Cts.IsCancellationRequested}");

                while (!Cts.IsCancellationRequested)
                {
                    ExecuteWithLogging(() => new(ResponseType.Info) { ResponseMessage = FlasherMessages.FlashBaseFirmware });

                    if (ExecuteWithLogging(() => new(ResponseType.Info) { ResponseMessage = "Waiting for CU signal..." }) is null
                        || ExecuteWithLogging(() => Flasher.SignalReady()) is not { ResponseType: ResponseType.OK }
                        || ExecuteWithLogging(() => SignalReady()) is null
                        || ExecuteWithLogging(() => Flasher.AwaitCUReady(Cts.Token)) is not { ResponseType: ResponseType.OK }
                        || ExecuteWithLogging(() => StartStopwatch()) is null
                        || ExecuteWithLogging(() => new(ResponseType.Info) { ResponseMessage = "Lock CU..." }) is null
                        || ExecuteWithLogging(() => Flasher.SignalBusy()) is not { ResponseType: ResponseType.OK }
                        || ExecuteWithLogging(() => SignalBusy()) is null
                        || ExecuteWithLogging(() => Flasher.LockCU()) is not { ResponseType: ResponseType.OK }
                        || ExecuteWithLogging(() => Flasher.Sleep(1)) is not { ResponseType: ResponseType.OK }
                        || ExecuteWithLogging(() => new(ResponseType.Info) { ResponseMessage = "Power on device..." }) is null
                        || ExecuteWithLogging(() => Flasher.TurnModemPowerOn()) is not { ResponseType: ResponseType.OK }
                        || ExecuteWithLogging(() => new(ResponseType.Info) { ResponseMessage = "Waiting for device..." }) is null
                        || ExecuteWithLogging(() => Flasher.AwaitDeviceAttach()) is not { ResponseType: ResponseType.OK }
                        || ExecuteWithLogging(() => new(ResponseType.Info) { ResponseMessage = "Waiting device boot up..." }) is null
                        || ExecuteWithLogging(() => Flasher.Sleep(BootUpTimeout)) is not { ResponseType: ResponseType.OK }
                        || ExecuteWithLogging(() => new(ResponseType.Info) { ResponseMessage = "Check device response..." }) is null
                        || ExecuteWithLogging(() => Flasher.CheckDeviceResponding()) is not { ResponseType: ResponseType.OK })
                    {
                        FailState();
                        continue;
                    }
                    else if (ExecuteWithLogging(() => new(ResponseType.Info) { ResponseMessage = "Check device is ADB..." }) is null
                            || ExecuteWithLogging(() => Flasher.CheckADBDevice()) is not { ResponseType: ResponseType.OK })
                    {
                        if (ExecuteWithLogging(() => new(ResponseType.Info) { ResponseMessage = "Turning on ADB interface..." }) is null
                            || ExecuteWithLogging(() => Flasher.TurnOnADBInterface()) is not { ResponseType: ResponseType.OK }
                            || ExecuteWithLogging(() => Flasher.Sleep(ADBSwitchTimeout)) is not { ResponseType: ResponseType.OK }
                            || ExecuteWithLogging(() => new(ResponseType.Info) { ResponseMessage = "Check if ADB is connected..." }) is null
                            || ExecuteWithLogging(() => Flasher.CheckADBDevice()) is not { ResponseType: ResponseType.OK })
                        {
                            FailState();
                            continue;
                        }
                    }
                    ExecuteWithLogging(() => Flasher.Sleep(2));
                    ExecuteWithLogging(() => new(ResponseType.Info) { ResponseMessage = "Running android batch..." });
                    if (ExecuteWithLogging(() => Flasher.Adb(Flasher.AdbRebootBootloaderCommand, BootloaderRebootTimeout)) is not { ResponseType: ResponseType.OK })
                    {
                        FailState();
                        continue;
                    }
                    ExecuteWithLogging(() => Flasher.Sleep(2));
                    if (ExecuteWithLogging(() => ExecuteFastboot()) is not { ResponseType: ResponseType.OK })
                    {
                        FailState();
                        continue;
                    }
                    ExecuteWithLogging(() => Flasher.Sleep(2));
                    if (ExecuteWithLogging(() => Flasher.Fastboot(Flasher.FastbootRebootCommand, 2)) is not { ResponseType: ResponseType.OK })
                    {
                        FailState();
                        continue;
                    }
                    if (Package is not null and { DeviceType: DeviceType.SimComSimple })
                    {
                        if (ExecuteWithLogging(() => new(ResponseType.Info) { ResponseMessage = "Waiting for device..." }) is null
                            || ExecuteWithLogging(() => Flasher.AwaitDeviceAttach()) is not { ResponseType: ResponseType.OK }
                            || ExecuteWithLogging(() => new(ResponseType.Info) { ResponseMessage = "Check device is ADB..." }) is null
                            || ExecuteWithLogging(() => Flasher.CheckADBDevice()) is not { ResponseType: ResponseType.OK })
                        {
                            ExecuteWithLogging(() => new(ResponseType.Info) { ResponseMessage = FlasherMessages.EnableADB });

                            if (ExecuteWithLogging(() => new(ResponseType.Info) { ResponseMessage = "Waiting device boot up..." }) is null
                                || ExecuteWithLogging(() => Flasher.Sleep(BootUpTimeout)) is not { ResponseType: ResponseType.OK }
                                || ExecuteWithLogging(() => new(ResponseType.Info) { ResponseMessage = "Check device response..." }) is null
                                || ExecuteWithLogging(() => Flasher.CheckDeviceResponding()) is not { ResponseType: ResponseType.OK }
                                || ExecuteWithLogging(() => new(ResponseType.Info) { ResponseMessage = "Turning on ADB interface..." }) is null
                                || ExecuteWithLogging(() => Flasher.TurnOnADBInterface()) is not { ResponseType: ResponseType.OK }
                                || ExecuteWithLogging(() => Flasher.Sleep(ADBSwitchTimeout)) is not { ResponseType: ResponseType.OK }
                                || ExecuteWithLogging(() => new(ResponseType.Info) { ResponseMessage = "Check if ADB is connected..." }) is null
                                || ExecuteWithLogging(() => Flasher.CheckADBDevice()) is not { ResponseType: ResponseType.OK })
                            {
                                FailState();
                                continue;
                            }
                        }
                        ExecuteWithLogging(() => new(ResponseType.Info) { ResponseMessage = FlasherMessages.UploadFactory });

                        if (ExecuteWithLogging(() => new(ResponseType.Info) { ResponseMessage = "Upload 'factory.cfg'..." }) is null
                            || ExecuteWithLogging(() => Flasher.Sleep(2)) is null
                            || ExecuteWithLogging(() => Flasher.UploadFactoryCFG()) is not { ResponseType: ResponseType.OK }
                            || ExecuteWithLogging(() => new(ResponseType.Info) { ResponseMessage = "Update 'factory.cfg'..." }) is null
                            || ExecuteWithLogging(() => Flasher.UpdateCfgSN()) is not { ResponseType: ResponseType.OK }
                            || ExecuteWithLogging(() => new(ResponseType.Info) { ResponseMessage = "Turning off ADB interface..." }) is null
                            || ExecuteWithLogging(() => Flasher.TurnOffADBInterface()) is not { ResponseType: ResponseType.OK })
                        {
                            FailState();
                            continue;
                        }
                    }
                    else if (Package is not null and { DeviceType: DeviceType.SimComRetro }
                            || Package is not null and { DeviceType: DeviceType.SimComFull })
                    {
                        if (ExecuteWithLogging(() => new(ResponseType.Info) { ResponseMessage = "Waiting for device..." }) is null
                            || ExecuteWithLogging(() => Flasher.AwaitDeviceAttach()) is not { ResponseType: ResponseType.OK }
                            || ExecuteWithLogging(() => new(ResponseType.Info) { ResponseMessage = "Check device is ADB..." }) is null
                            || ExecuteWithLogging(() => Flasher.CheckADBDevice()) is { ResponseType: ResponseType.OK })
                        {
                            if (ExecuteWithLogging(() => new(ResponseType.Info) { ResponseMessage = "Waiting device boot up..." }) is null
                                || ExecuteWithLogging(() => Flasher.Sleep(BootUpTimeout)) is not { ResponseType: ResponseType.OK }
                                || ExecuteWithLogging(() => new(ResponseType.Info) { ResponseMessage = "Check device response..." }) is null
                                || ExecuteWithLogging(() => Flasher.CheckDeviceResponding()) is not { ResponseType: ResponseType.OK }
                                || ExecuteWithLogging(() => new(ResponseType.Info) { ResponseMessage = "Turning off ADB interface..." }) is null
                                || ExecuteWithLogging(() => Flasher.TurnOffADBInterface()) is not { ResponseType: ResponseType.OK })
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
                    ExecuteWithLogging(() => Flasher.TurnModemPowerOff());
                    ExecuteWithLogging(() => Flasher.UnlockCU());
                    ExecuteWithLogging(() => StopStopwatch());
                    EventOccurred?.Invoke(this, new(FlasherControllerEventType.SuccessState) { Payload = ReadSerial(Package) });
                    Log("Some success ;]");
                    ExecuteWithLogging(() => new(ResponseType.Info) { ResponseMessage = "Waiting Contact Unit release..." });
                    ExecuteWithLogging(() => Flasher.SignalDone());
                    ExecuteWithLogging(() => Flasher.AwaitCURelease(Cts.Token));
                    ExecuteWithLogging(() => ResetStopwatch());
                }

                void FailState()
                {
                    ExecuteWithLogging(() => Flasher.TurnModemPowerOff());
                    ExecuteWithLogging(() => Flasher.UnlockCU());
                    ExecuteWithLogging(() => StopStopwatch());
                    EventOccurred?.Invoke(this, new(FlasherControllerEventType.FailState));
                    Log("FAIL!");
                    ExecuteWithLogging(() => new(ResponseType.Info) { ResponseMessage = "Waiting Contact Unit release..." });
                    ExecuteWithLogging(() => Flasher.SignalFail());
                    ExecuteWithLogging(() => Flasher.AwaitCURelease(Cts!.Token));
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
                    foreach (var command in Flasher.FastbootCommandSequence)
                    {
                        if (ExecuteWithLogging(() => Flasher.Fastboot(command.Key, command.Value)) is not { ResponseType: ResponseType.OK })
                            return new(ResponseType.Fail) { ResponseMessage = command.Key };
                    }
                    return new(ResponseType.OK) { ResponseMessage = nameof(Flasher.FastbootCommandSequence) };
                }

                EventOccurred?.Invoke(this, new(FlasherControllerEventType.FlasherStateChanged) { Payload = false }); // Dispatcher.UIThread.Invoke(() => IsFlasherRunning = false);
                Flasher.Dispose();
            }
            ,
            TechnolabsFlasher => delegate ()
            {
                Flasher.Package = Package;

                EventOccurred?.Invoke(this, new(FlasherControllerEventType.FlasherStateChanged) { Payload = false });

                Log("START:");
                Log("-= 4 seconds pause for FTDI =-");
                ExecuteWithLogging(() => Flasher.Sleep(4));

                string file;

                try
                {
                    file = Directory.EnumerateFiles(Package.PackagePath, "*H_Nor.blf").First();
                }
                catch (Exception ex)
                {
                    Log(ex.Message);
                    return;
                }

                var swdconsoleCommand = string.Join(" ", "-f", file);

                if (Cts.IsCancellationRequested)
                    Log($"Cancellation requested. {nameof(Cts)} Token is {Cts.IsCancellationRequested}");

                while (!Cts.IsCancellationRequested)
                {
                    ExecuteWithLogging(() => new(ResponseType.Info) { ResponseMessage = FlasherMessages.FlashBaseFirmware });

                    if (ExecuteWithLogging(() => new(ResponseType.Info) { ResponseMessage = "Waiting for CU signal..." }) is null
                        || ExecuteWithLogging(() => Flasher.SignalReady()) is not { ResponseType: ResponseType.OK }
                        || ExecuteWithLogging(() => SignalReady()) is null
                        || ExecuteWithLogging(() => Flasher.AwaitCUReady(Cts.Token)) is not { ResponseType: ResponseType.OK }
                        || ExecuteWithLogging(() => StartStopwatch()) is null
                        || ExecuteWithLogging(() => new(ResponseType.Info) { ResponseMessage = "Lock CU..." }) is null
                        || ExecuteWithLogging(() => Flasher.SignalBusy()) is not { ResponseType: ResponseType.OK }
                        || ExecuteWithLogging(() => SignalBusy()) is null
                        || ExecuteWithLogging(() => Flasher.LockCU()) is not { ResponseType: ResponseType.OK }
                        || ExecuteWithLogging(() => Flasher.Sleep(1)) is not { ResponseType: ResponseType.OK }
                        || ExecuteWithLogging(() => new(ResponseType.Info) { ResponseMessage = "Power on device..." }) is null
                        || ExecuteWithLogging(() => Flasher.TurnModemPowerOn()) is not { ResponseType: ResponseType.OK }
                        || ExecuteWithLogging(() => new(ResponseType.Info) { ResponseMessage = "Waiting for device..." }) is null
                        || ExecuteWithLogging(() => Flasher.AwaitDeviceAttach()) is not { ResponseType: ResponseType.OK }
                        || ExecuteWithLogging(() => new(ResponseType.Info) { ResponseMessage = "Writing to device..." }) is null
                        || ExecuteWithLogging(() => Flasher.SWDConsole(swdconsoleCommand, SWDConsoleTimeout, Log)) is not { ResponseType: ResponseType.OK })
                    {
                        FailState();
                        continue;
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
                    ExecuteWithLogging(() => Flasher.TurnModemPowerOff());
                    ExecuteWithLogging(() => Flasher.UnlockCU());
                    ExecuteWithLogging(() => StopStopwatch());
                    EventOccurred?.Invoke(this, new(FlasherControllerEventType.SuccessState) { Payload = ReadSerial(Package) });
                    Log("Some success ;]");
                    ExecuteWithLogging(() => new(ResponseType.Info) { ResponseMessage = "Waiting Contact Unit release..." });
                    ExecuteWithLogging(() => Flasher.SignalDone());
                    ExecuteWithLogging(() => Flasher.AwaitCURelease(Cts.Token));
                    ExecuteWithLogging(() => ResetStopwatch());
                }

                void FailState()
                {
                    ExecuteWithLogging(() => Flasher.TurnModemPowerOff());
                    ExecuteWithLogging(() => Flasher.UnlockCU());
                    ExecuteWithLogging(() => StopStopwatch());
                    EventOccurred?.Invoke(this, new(FlasherControllerEventType.FailState));
                    Log("FAIL!");
                    ExecuteWithLogging(() => new(ResponseType.Info) { ResponseMessage = "Waiting Contact Unit release..." });
                    ExecuteWithLogging(() => Flasher.SignalFail());
                    ExecuteWithLogging(() => Flasher.AwaitCURelease(Cts!.Token));
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

                EventOccurred?.Invoke(this, new(FlasherControllerEventType.FlasherStateChanged) { Payload = false }); // Dispatcher.UIThread.Invoke(() => IsFlasherRunning = false);
                Flasher.Dispose();
            }
            ,
            _ => Task.CompletedTask.Wait
            ,
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

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                // TODO: dispose managed state (managed objects)
                Flasher?.Dispose();
                timer.Dispose();
                Cts?.Dispose();
            }

            // TODO: free unmanaged resources (unmanaged objects) and override finalizer
            // TODO: set large fields to null
            disposedValue = true;
        }
    }

    // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
    // ~FlasherController()
    // {
    //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
    //     Dispose(disposing: false);
    // }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}

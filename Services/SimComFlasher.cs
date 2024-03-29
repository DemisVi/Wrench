using System;
using System.Linq;
using System.Management;
using Iot.Device.Ft2232H;
using Iot.Device.FtCommon;
using Wrench.Models;
using Wrench.DataTypes;
using System.Threading;
using System.Diagnostics;
using System.Text;
using DynamicData;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace Wrench.Services;

public class SimComFlasher : IFlasher, IDisposable
{
    private bool disposedValue;
    private readonly ContactUnit cu;
    private readonly Adb adb;
    private readonly Fastboot fastboot;
    private readonly GpioInputs deviceCUReadyState = GpioInputs.Lodg | GpioInputs.Device | GpioInputs.Pn1_Down,
        deviceCUSignalState = GpioInputs.Lodg,
        deviceCUReleaseState = GpioInputs.Pn1_Down;
    private const string adbOn = "AT+CUSBADB=1,1",
        adbOff = "AT+CUSBADB=0,1",
        factory = "factory.cfg",
        adbPushFormat = "push \"{0}\" /data/",
        adbSync = "shell sync",
        adbKill = "kill-server", 
        adbStart = "start-server";

    private string workDir = string.Empty;
    private Package? package;

    public SimComFlasher()
    {
        var ftDevice = new Ft2232HDevice(
            Ftx232HDevice.GetFtx232H().First(
                x => x.SerialNumber.EndsWith("A", StringComparison.OrdinalIgnoreCase)));
        var ftPortName = FtSerialPort.GetPortNames().First();
        var ftSerialPort = new FtSerialPort(ftPortName);

        adb = new Adb();
        fastboot = new Fastboot();
        cu = new ContactUnit(ftSerialPort, ftDevice);
    }

    public string WorkingDir
    {
        get { return workDir; }
        private set
        {
            workDir = value;
            adb.WorkingDir = value;
            fastboot.WorkingDir = value;
        }
    }
    public Package? Package
    {
        get => package;
        set
        {
            package = value;
            WorkingDir = value?.PackagePath ?? string.Empty;
        }
    }
    public string AdbRebootBootloaderCommand { get; } = "reboot bootloader";
    public IEnumerable<KeyValuePair<string, int>> FastbootCommandSequence { get; set; } = new KeyValuePair<string, int>[]
    {
        new("flash aboot appsboot.mbn", 2),
        new("flash rpm rpm.mbn", 2),
        new("flash sbl sbl1.mbn", 2),
        new("flash tz tz.mbn", 2),
        new("flash modem modem.img", 15),
        new("flash boot boot.img", 3),
        new("flash system system.img", 18),
        new("flash recovery  recovery.img", 3),
        new("flash recoveryfs recoveryfs.img", 4),
    };
    public string FastbootRebootCommand { get; } = "reboot";
    public int DeviceWaitTime { get; set; } = 20;
    public Func<string, int, FlasherResponse> Adb => delegate (string command, int timeout)
    {
        var res = adb.Run(command, timeout);
        return new((ResponseType)res) { ResponseMessage = $"Adb {command}\n\tStdOut: {adb.LastStdOut}\n\tStdErr: {adb.LastStdErr}" };
    };
    public Func<string, int, FlasherResponse> Fastboot => delegate (string command, int timeout)
    {
        var res = fastboot.Run(command, timeout);
        return new((ResponseType)res) { ResponseMessage = $"Fastboot {command}\n\tStdOut: {fastboot.LastStdOut}\n\tStdErr: {fastboot.LastStdErr}" };
    };

    public Func<string, int, Action<string>?, FlasherResponse> SWDConsole => throw new NotImplementedException();

    public FlasherResponse Sleep(int timeoutSeconds) // ok 
    {
        Thread.Sleep(TimeSpan.FromSeconds(timeoutSeconds));
        return new FlasherResponse(ResponseType.OK) { ResponseMessage = FlasherMessages.Delay + timeoutSeconds };
    }

    public FlasherResponse AwaitCUReady(CancellationToken token) => AwaitCUState(GetCUReady, token);

    public FlasherResponse AwaitCUSignal(CancellationToken token) => AwaitCUState(GetCUSignal, token);

    public FlasherResponse AwaitCURelease(CancellationToken token) => AwaitCUState(GetCURelease, token);

    public FlasherResponse AwaitCUState(Func<FlasherResponse> func, CancellationToken token) // ok 
    {
        try
        {
            while (!token.IsCancellationRequested)
            {
                var state = func();
                if (state is { ResponseType: ResponseType.OK }) return state;
                else
                    Thread.Sleep(200);
            }
            return new FlasherResponse(ResponseType.Fail) { ResponseMessage = FlasherMessages.CancellationRequested, };
        }
        catch (Exception ex)
        {
            return new FlasherResponse(ex);
        }
    }

    public FlasherResponse GetCUReady() => GetCUState(deviceCUReadyState);

    public FlasherResponse GetCUSignal() => GetCUState(deviceCUSignalState);

    public FlasherResponse GetCURelease() => GetCUState(deviceCUReleaseState);

    public FlasherResponse GetCUState(GpioInputs inputs) // ok 
    {
        try
        {
            var ins = cu.Inputs;
            if (ins == inputs)
                return new FlasherResponse(ResponseType.OK)
                {
                    ResponseMessage = FlasherMessages.ContactUnitState + ins.ToString()
                };
            else
                return new FlasherResponse(ResponseType.Fail)
                {
                    ResponseMessage = FlasherMessages.ContactUnitState + ins.ToString()
                };

        }
        catch (Exception ex)
        {
            return new FlasherResponse(ResponseType.Fail) { ResponseMessage = ex.Message };
        }
    }

    public FlasherResponse AwaitDeviceAttach() // ok 
    {
#pragma warning disable CA1416

        using var watcher = new ManagementEventWatcher(WqlQueries.CreationSimCom);
        watcher.Options.Timeout = TimeSpan.FromSeconds(DeviceWaitTime);
        try
        {
            watcher.Start();
            var res = watcher.WaitForNextEvent();
            watcher.Stop();

            return new FlasherResponse(ResponseType.OK)
            {
                ResponseMessage = FlasherMessages.DeviceFound
                    + ((ManagementBaseObject)res["TargetInstance"])?["Caption"],
            };
        }
        catch (ManagementException ex)
        {
            return new FlasherResponse(ex);
        }
        finally
        {
            watcher.Stop();
        }

#pragma warning restore CA1416

    }

    public FlasherResponse CheckDeviceResponding() // ok 
    {
        var port = ModemPort.GetModemATPortNames().FirstOrDefault();
        if (string.IsNullOrEmpty(port)) return new FlasherResponse(ResponseType.Fail) { ResponseMessage = FlasherMessages.ModemPortNotFound, };

        using var modemPort = new ModemPort(port);
        modemPort.Open();
        modemPort.DiscardInBuffer();
        modemPort.WriteLine("AT");
        Thread.Sleep(200);
        var response = modemPort.ReadExisting().Replace('\n', ' ').Replace('\r', ' ');

        modemPort.Dispose();

        if (!response.Contains("OK", StringComparison.OrdinalIgnoreCase))
            return new FlasherResponse(ResponseType.Fail) { ResponseMessage = response };
        else
            return new FlasherResponse(ResponseType.OK) { ResponseMessage = response };
    }

    public FlasherResponse CheckADBDevice() // ok 
    {
#pragma warning disable CA1416

        using var searcher = new ManagementObjectSearcher(WqlQueries.ObjectAndroidDevice);

        try
        {
            var res = searcher.Get();
            Thread.Sleep(1000);
            if (res is not null and { Count: > 0 })
            {
                var text = (string?)res.Cast<ManagementObject>()?.First()["Caption"];
                return new FlasherResponse() { ResponseMessage = text, ResponseType = ResponseType.OK };
            }
        }
        catch (Exception ex)
        {
            return new FlasherResponse(ex);
        }
#pragma warning restore CA1416

        return new FlasherResponse(ResponseType.Fail) { ResponseMessage = FlasherMessages.DeviceNotFound };
    }

    public FlasherResponse ExecuteFastbootBatch()
    {
        return new(ResponseType.Info) { ResponseMessage = "TEST MODE", };
    }

    public FlasherResponse FlasherState()
    {
        throw new NotImplementedException();
    }

    public FlasherResponse LockCU() // ok 
    {
        try
        {
            cu.LockBoard();
        }
        catch (Exception ex)
        {
            return new FlasherResponse(ex);
        }
        return new FlasherResponse(ResponseType.OK) { ResponseMessage = FlasherMessages.LockCU + cu.Outputs };
    }

    public FlasherResponse UnlockCU() // ok 
    {
        try
        {
            cu.ReleaseBoard();
        }
        catch (Exception ex)
        {
            return new FlasherResponse(ex);
        }
        return new FlasherResponse(ResponseType.OK) { ResponseMessage = FlasherMessages.UnlockCU + cu.Outputs };
    }

    public FlasherResponse SignalReady() // ok
    {
        try
        {
            cu.LEDYellow();
        }
        catch (Exception ex)
        {
            return new FlasherResponse(ex);
        }
        return new FlasherResponse(ResponseType.OK) { ResponseMessage = FlasherMessages.CUReady + cu.Outputs };
    }

    public FlasherResponse SignalDone()
    {
        try
        {
            cu.LEDGreen();
        }
        catch (Exception ex)
        {
            return new FlasherResponse(ex);
        }
        return new FlasherResponse(ResponseType.OK) { ResponseMessage = FlasherMessages.Done + cu.Outputs };
    }

    public FlasherResponse SignalBusy()
    {
        try
        {
            cu.LEDBlue();
        }
        catch (Exception ex)
        {
            return new FlasherResponse(ex);
        }
        return new FlasherResponse(ResponseType.OK) { ResponseMessage = FlasherMessages.Busy + cu.Outputs };
    }

    public FlasherResponse SignalFail()
    {
        try
        {
            cu.LEDRed();
        }
        catch (Exception ex)
        {
            return new FlasherResponse(ex);
        }
        return new FlasherResponse(ResponseType.OK) { ResponseMessage = FlasherMessages.Fail + cu.Outputs };
    }

    public FlasherResponse TurnModemPowerOff() // ok 
    {
        try
        {
            cu.PowerOffBoard();
            cu.Cl15Off();
        }
        catch (Exception ex)
        {
            return new FlasherResponse(ex);
        }
        return new FlasherResponse(ResponseType.OK) { ResponseMessage = FlasherMessages.PowerOff };
    }

    public FlasherResponse TurnModemPowerOn() // ok 
    {
        try
        {
            cu.PowerOnBoard();
            cu.Cl15On();
        }
        catch (Exception ex)
        {
            return new FlasherResponse(ex);
        }
        return new FlasherResponse(ResponseType.OK) { ResponseMessage = FlasherMessages.PowerOn };
    }

    public FlasherResponse TurnOffADBInterface() => SwitchADB(adbOff);

    public FlasherResponse TurnOnADBInterface() => SwitchADB(adbOn);
    protected FlasherResponse SwitchADB(string req) // Piece of shit
    {
        var portName = ModemPort.GetModemATPortNames().FirstOrDefault();

        if (portName is null or { Length: <= 0 })
            return new FlasherResponse(ResponseType.Fail) { ResponseMessage = FlasherMessages.ModemPortNotFound };

        var port = new ModemPort(portName);

        try
        {
            port.Open();
            Thread.Sleep(100);
            port.WriteLine(req);

            return new FlasherResponse(ResponseType.OK) { ResponseMessage = FlasherMessages.CantReadAnswer };
        }
        catch (Exception ex)
        {
            return new FlasherResponse(ex);
        }
        finally
        {
            port.Close();
        }

    }

    public FlasherResponse UpdateCfgSN()
    {
        var fac = GetFactoryPath();

        if (File.Exists(fac))
        {
            try
            {
                var factory = new FactoryCFG(Path.GetDirectoryName(fac));
                factory.UpdateFactory();
            }
            catch (Exception ex)
            {
                return new(ResponseType.Fail) { ResponseMessage = ex.Message };
            }
            return new(ResponseType.OK) { ResponseMessage = FlasherMessages.FactoryUpdated };
        }
        else
            return new(ResponseType.Fail) { ResponseMessage = FlasherMessages.FactoryDoesntExist };
    }

    public FlasherResponse UploadFactoryCFG()
    {

        var fac = GetFactoryPath();

        if (File.Exists(fac))
        {
            var res = Adb(string.Format(adbPushFormat, fac), 1);
            Adb(adbSync, 5);
            return res;
        }
        else
            return new(ResponseType.Fail) { ResponseMessage = string.Format(FlasherMessages.FileNotFoundFormat, fac) };
    }

    protected string GetFactoryPath()
    {
        string path = string.Empty;
        if (Package is not null)
            path = Path.GetDirectoryName(Package.PackagePath)!;
        return Path.Combine(path, factory);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                // TODO: dispose managed state (managed objects)

                cu.Dispose();
            }

            Adb(adbKill, 1);
            // TODO: free unmanaged resources (unmanaged objects) and override finalizer
            // TODO: set large fields to null
            disposedValue = true;
        }
    }

    // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
    // ~SimComFlasher()
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

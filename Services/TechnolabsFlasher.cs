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

public class TechnolabsFlasher : IFlasher, IDisposable
{
    private bool disposedValue;
    private readonly ContactUnit cu;
    private readonly GpioInputs deviceCUReadyState = GpioInputs.Lodg | GpioInputs.Device | GpioInputs.Pn1_Down,
        deviceCUSignalState = GpioInputs.Lodg,
        deviceCUReleaseState = GpioInputs.Pn1_Down;

    private string workDir = string.Empty;
    private Package? package;

    public TechnolabsFlasher()
    {
        var ftDevice = new Ft2232HDevice(
            Ftx232HDevice.GetFtx232H().First(
                x => x.SerialNumber.EndsWith("A", StringComparison.OrdinalIgnoreCase)));
        var ftPortName = FtSerialPort.GetPortNames().First();
        var ftSerialPort = new FtSerialPort(ftPortName);

        cu = new ContactUnit(ftSerialPort, ftDevice);
    }

    public string WorkingDir
    {
        get { return workDir; }
        private set
        {
            workDir = value;
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
    public int DeviceWaitTime { get; set; } = 20;

    public Func<string, int, FlasherResponse> Adb => throw new NotImplementedException();

    public Func<string, int, FlasherResponse> Fastboot => throw new NotImplementedException();

    public string AdbRebootBootloaderCommand => throw new NotImplementedException();

    public IEnumerable<KeyValuePair<string, int>> FastbootCommandSequence { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

    public string FastbootRebootCommand => throw new NotImplementedException();

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

    public FlasherResponse AwaitDeviceAttach()
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

    public FlasherResponse CheckDeviceResponding()
    {
        throw new NotImplementedException();
    }

    public FlasherResponse CheckADBDevice()
    {
        throw new NotImplementedException();
    }

    public FlasherResponse ExecuteFastbootBatch()
    {
        throw new NotImplementedException();
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

    public FlasherResponse TurnOffADBInterface()
    {
        throw new NotImplementedException();
    }

    public FlasherResponse TurnOnADBInterface()
    {
        throw new NotImplementedException();
    }

    public FlasherResponse UpdateCfgSN()
    {
        throw new NotImplementedException();
    }

    public FlasherResponse UploadFactoryCFG()
    {
        throw new NotImplementedException();
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

            // TODO: free unmanaged resources (unmanaged objects) and override finalizer
            // TODO: set large fields to null
            disposedValue = true;
        }
    }

    // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
    // ~TechnolabsFlasher()
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

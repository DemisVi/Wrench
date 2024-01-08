using System;
using System.Linq;
using System.Management;
using Iot.Device.Ft2232H;
using Iot.Device.FtCommon;
using Wrench.Models;
using Wrench.DataTypes;
using System.Threading;

namespace Wrench.Services;

public class Flasher : IFlasher, IDisposable
{
    private bool disposedValue;
    private readonly ContactUnit cu;
    private readonly GpioInputs deviceCUReadyState = GpioInputs.Lodg | GpioInputs.Device | GpioInputs.Pn1_Up;
    private readonly GpioInputs deviceCUSignalState = GpioInputs.Lodg;
    private const string modemPortNotFoundMessage = "Modem Port not found";
    private const string cancellationRequestedMessage = "Cancellation requested";
    private const string deviceNotFoundMessage = "Device not found";

    public Flasher()
    {
        var ftDevice = new Ft2232HDevice(
            Ftx232HDevice.GetFtx232H().First(
                x => x.SerialNumber.EndsWith("A", StringComparison.OrdinalIgnoreCase)));
        var ftPortName = FtSerialPort.GetPortNames().First();
        var ftSerialPort = new FtSerialPort(ftPortName);

        cu = new ContactUnit(ftSerialPort, ftDevice);
    }

    public int DeviceWaitTime { get; set; } = 20;

    public FlasherResponce Sleep(int timeoutSeconds) // ok 
    {
        Thread.Sleep(TimeSpan.FromSeconds(timeoutSeconds));
        return new FlasherResponce(ResponceType.OK);
    }

    public FlasherResponce AwaitCUReady(CancellationToken token) => AwaitCUState(GetCUReady, token);

    public FlasherResponce AwaitCUSignal(CancellationToken token) => AwaitCUState(GetCUSignal, token);

    public FlasherResponce AwaitCUState(Func<FlasherResponce> func, CancellationToken token) // ok 
    {
        try
        {
            while (!token.IsCancellationRequested)
            {
                var state = func();
                if (state is { ResponceType: ResponceType.OK }) return state;
                else
                    Thread.Sleep(200);
            }
            return new FlasherResponce(ResponceType.Unsuccess) { ResponceMessage = cancellationRequestedMessage, };
        }
        catch (Exception ex)
        {
            return new FlasherResponce(ex);
        }
    }

    public FlasherResponce GetCUReady() => GetCUState(deviceCUReadyState);

    public FlasherResponce GetCUSignal() => GetCUState(deviceCUSignalState);

    public FlasherResponce GetCUState(GpioInputs inputs) // ok 
    {
        try
        {
            if (cu.Inputs == inputs)
                return new FlasherResponce(ResponceType.OK) { ResponceMessage = cu.Inputs.ToString() };
            else
                return new FlasherResponce(ResponceType.Fail) { ResponceMessage = cu.Inputs.ToString() };

        }
        catch (Exception ex)
        {
            return new FlasherResponce(ResponceType.Unsuccess) { ResponceMessage = ex.Message };
        }
    }

    public FlasherResponce AwaitDeviceAttach() // ok 
    {
#pragma warning disable CA1416

        using var watcher = new ManagementEventWatcher(WqlQueries.CreationSimCom);
        watcher.Options.Timeout = TimeSpan.FromSeconds(DeviceWaitTime);
        try
        {
            watcher.Start();
            var res = watcher.WaitForNextEvent();
            watcher.Stop();

            return new FlasherResponce(ResponceType.OK) 
            { 
                ResponceMessage = (string?)((ManagementBaseObject)res["TargetInstance"])?["Caption"] ?? "",
            };
        }
        catch (ManagementException ex)
        {
            return new FlasherResponce(ex);
        }
        finally
        {
            watcher.Stop();
        }

#pragma warning restore CA1416

    }

    public FlasherResponce AwaitDeviceStart() // ok 
    {
        var port = ModemPort.GetModemATPortNames().FirstOrDefault();
        if (string.IsNullOrEmpty(port)) return new FlasherResponce(ResponceType.Fail) { ResponceMessage = modemPortNotFoundMessage, };

        using var modemPort = new ModemPort(port);
        modemPort.Open();
        modemPort.DiscardInBuffer();
        modemPort.WriteLine("AT");
        Thread.Sleep(200);
        var response = modemPort.ReadExisting().Replace('\n', ' ').Replace('\r', ' ');

        modemPort.Dispose();

        if (!response.Contains("OK", StringComparison.OrdinalIgnoreCase))
            return new FlasherResponce(ResponceType.Fail) { ResponceMessage = response };
        else
            return new FlasherResponce(ResponceType.OK) { ResponceMessage = response };
    }

    public FlasherResponce CheckADBDevice() // ok 
    {
#pragma warning disable CA1416

        using var searcher = new ManagementObjectSearcher(WqlQueries.ObjectAndroidDevice);

        try
        {
            var res = searcher.Get();
            if (res is not null and { Count: > 0 })
            {
                var text = (string?)res.Cast<ManagementObject>()?.First()["Caption"];
                return new FlasherResponce() { ResponceMessage = text, ResponceType = ResponceType.OK };
            }
        }
        catch (Exception ex)
        {
            return new FlasherResponce(ex);
        }
#pragma warning restore CA1416

        return new FlasherResponce(ResponceType.Fail) { ResponceMessage = deviceNotFoundMessage };
    }

    public FlasherResponce ExecuteFastbootBatch()
    {
        throw new NotImplementedException();
    }

    public FlasherResponce FlasherState()
    {
        throw new NotImplementedException();
    }

    public FlasherResponce LockCU() // ok 
    {
        try
        {
            cu.LockBoard();
        }
        catch (Exception ex)
        {
            return new FlasherResponce(ex);
        }
        return new FlasherResponce(ResponceType.OK);
    }

    public FlasherResponce UnlockCU() // ok 
    {
        try
        {
            cu.ReleaseBoard();
        }
        catch (Exception ex)
        {
            return new FlasherResponce(ex);
        }
        return new FlasherResponce(ResponceType.OK);
    }

    public FlasherResponce SignalReady() // ok
    {
        try
        {
            cu.LEDYellow();
        }
        catch (Exception ex)
        {
            return new FlasherResponce(ex);
        }
        return new FlasherResponce(ResponceType.OK);
    }

    public FlasherResponce TurnModemPowerOff() // ok 
    {
        try
        {
            cu.PowerOffBoard();
        }
        catch (Exception ex)
        {
            return new FlasherResponce(ex);
        }
        return new FlasherResponce(ResponceType.OK);
    }

    public FlasherResponce TurnModemPowerOn() // ok 
    {
        try
        {
            cu.PowerOnBoard();
        }
        catch (Exception ex)
        {
            return new FlasherResponce(ex);
        }
        return new FlasherResponce(ResponceType.OK);
    }

    public FlasherResponce TurnOffADBInterface()
    {
        throw new NotImplementedException();
    }

    public FlasherResponce TurnOnADBInterface()
    {
        throw new NotImplementedException();
    }

    public FlasherResponce UpdateCfgSN()
    {
        throw new NotImplementedException();
    }

    public FlasherResponce UploadFactoryCFG()
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
    // ~Flasher()
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

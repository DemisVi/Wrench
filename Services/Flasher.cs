using System;
using System.Linq;
using System.Management;
using Iot.Device.Ft2232H;
using Iot.Device.FtCommon;
using Wrench.Models;
using Wrench.DataTypes;
using System.Threading;
using System.Diagnostics;

namespace Wrench.Services;

public class Flasher : IFlasher, IDisposable
{
    private bool disposedValue;
    private readonly ContactUnit cu;
    private readonly GpioInputs deviceCUReadyState = GpioInputs.Lodg | GpioInputs.Device | GpioInputs.Pn1_Up;
    private readonly GpioInputs deviceCUSignalState = GpioInputs.Lodg;
    private const string modemPortNotFoundMessage = "Modem Port not found",
        cancellationRequestedMessage = "Cancellation requested",
        deviceNotFoundMessage = "Device not found",
        adbOn = "AT+CUSBADB=1,1",
        adbOff = "AT+CUSBADB=0,1";


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

    public FlasherResponse Sleep(int timeoutSeconds) // ok 
    {
        Thread.Sleep(TimeSpan.FromSeconds(timeoutSeconds));
        return new FlasherResponse(ResponseType.OK);
    }

    public FlasherResponse AwaitCUReady(CancellationToken token) => AwaitCUState(GetCUReady, token);

    public FlasherResponse AwaitCUSignal(CancellationToken token) => AwaitCUState(GetCUSignal, token);

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
            return new FlasherResponse(ResponseType.Unsuccess) { ResponseMessage = cancellationRequestedMessage, };
        }
        catch (Exception ex)
        {
            return new FlasherResponse(ex);
        }
    }

    public FlasherResponse GetCUReady() => GetCUState(deviceCUReadyState);

    public FlasherResponse GetCUSignal() => GetCUState(deviceCUSignalState);

    public FlasherResponse GetCUState(GpioInputs inputs) // ok 
    {
        try
        {
            if (cu.Inputs == inputs)
                return new FlasherResponse(ResponseType.OK) { ResponseMessage = cu.Inputs.ToString() };
            else
                return new FlasherResponse(ResponseType.Fail) { ResponseMessage = cu.Inputs.ToString() };

        }
        catch (Exception ex)
        {
            return new FlasherResponse(ResponseType.Unsuccess) { ResponseMessage = ex.Message };
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
                ResponseMessage = (string?)((ManagementBaseObject)res["TargetInstance"])?["Caption"] ?? "",
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

    public FlasherResponse AwaitDeviceStart() // ok 
    {
        var port = ModemPort.GetModemATPortNames().FirstOrDefault();
        if (string.IsNullOrEmpty(port)) return new FlasherResponse(ResponseType.Fail) { ResponseMessage = modemPortNotFoundMessage, };

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

        return new FlasherResponse(ResponseType.Fail) { ResponseMessage = deviceNotFoundMessage };
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
        return new FlasherResponse(ResponseType.OK);
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
        return new FlasherResponse(ResponseType.OK);
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
        return new FlasherResponse(ResponseType.OK);
    }

    public FlasherResponse TurnModemPowerOff() // ok 
    {
        try
        {
            cu.PowerOffBoard();
        }
        catch (Exception ex)
        {
            return new FlasherResponse(ex);
        }
        return new FlasherResponse(ResponseType.OK);
    }

    public FlasherResponse TurnModemPowerOn() // ok 
    {
        try
        {
            cu.PowerOnBoard();
        }
        catch (Exception ex)
        {
            return new FlasherResponse(ex);
        }
        return new FlasherResponse(ResponseType.OK);
    }

    public FlasherResponse TurnOffADBInterface() => SwitchADB(adbOff);

    public FlasherResponse TurnOnADBInterface() => SwitchADB(adbOn);
    protected FlasherResponse SwitchADB(string req)
    {
        var portName = ModemPort.GetModemATPortNames().FirstOrDefault();
        if (portName is null or { Length: <= 0 })
            return new FlasherResponse(ResponseType.Fail) { ResponseMessage = "Modem Port not found" };

        var port = new ModemPort(portName);

        try
        {
            port.Open();

            port.TryGetResponce(req, out var resp);

            return new FlasherResponse(ResponseType.OK) { ResponseMessage = resp, };
        }
        catch (Exception ex)
        {
            return new FlasherResponse(ex);
        }

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

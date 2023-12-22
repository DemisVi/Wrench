using System;
using System.Linq;
using System.Management;
using Iot.Device.Ft2232H;
using Iot.Device.FtCommon;
using Wrench.Models;

namespace Wrench.Services;

public class Flasher : IFlasher
{
    private ContactUnit cu;
    public Flasher()
    {
        var ftDevice = new Ft2232HDevice(
            Ftx232HDevice.GetFtx232H().First(
                x => x.SerialNumber.EndsWith("A", StringComparison.OrdinalIgnoreCase)));
        var ftPortName = FtSerialPort.GetPortNames().First();
        var ftSerialPort = new FtSerialPort(ftPortName);

        cu = new ContactUnit(ftSerialPort, ftDevice);
    }
    public void AwaitCUClose()
    {
        throw new NotImplementedException();
        // cu.
    }

    public void AwaitCUSignal()
    {
        throw new NotImplementedException();
    }

    public void AwaitDeviceAttach()
    {
#pragma warning disable CA1416

        var watcher = new ManagementEventWatcher(WqlQueries.CreationSimCom);
        watcher.Options.Timeout = TimeSpan.FromSeconds(20);
        try
        {
            watcher.Start();
            watcher.WaitForNextEvent();
            watcher.Stop();
        }
        catch (Exception)
        {
            System.Console.WriteLine("allFucked!!");
            throw;
        }
        
#pragma warning restore CA1416
    }

    public void AwaitDeviceStart()
    {
        throw new NotImplementedException();
    }

    public void CheckADBDevice()
    {
        throw new NotImplementedException();
    }

    public void ExecuteFastbootBatch()
    {
        throw new NotImplementedException();
    }

    public void FlasherState()
    {
        throw new NotImplementedException();
    }

    public void LockCU() => cu.LockBoard();
    public void UnlockCU() => cu.ReleaseBoard();

    public void SignalReady() => cu.LEDYellow();

    public void TurnModemPowerOff() => cu.PowerOffBoard();

    public void TurnModemPowerOn() => cu.PowerOnBoard();

    public void TurnOffADBInterface()
    {
        throw new NotImplementedException();
    }

    public void TurnOnADBInterface()
    {
        throw new NotImplementedException();
    }

    public void UpdateCfgSN()
    {
        throw new NotImplementedException();
    }

    public void UploadFactoryCFG()
    {
        throw new NotImplementedException();
    }
}

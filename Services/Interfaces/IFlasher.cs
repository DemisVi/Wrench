using System;
using Wrench.Models;
using System.Threading;

namespace Wrench.Services;

public interface IFlasher
{
    public FlasherResponse SignalReady();
    public FlasherResponse GetCUReady();
    public FlasherResponse AwaitCUReady(CancellationToken token);
    public FlasherResponse AwaitCUSignal(CancellationToken token);
    public FlasherResponse LockCU();
    public FlasherResponse TurnModemPowerOn();
    public FlasherResponse TurnModemPowerOff();
    public FlasherResponse AwaitDeviceAttach();
    public FlasherResponse CheckDeviceResponding();
    public FlasherResponse TurnOnADBInterface();
    public FlasherResponse TurnOffADBInterface();
    public FlasherResponse ExecuteFastbootBatch();
    public FlasherResponse UpdateCfgSN();
    public FlasherResponse UploadFactoryCFG();
    public FlasherResponse FlasherState();
    public FlasherResponse CheckADBDevice();
}

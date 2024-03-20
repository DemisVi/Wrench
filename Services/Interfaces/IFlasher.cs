using System;
using Wrench.Models;
using System.Threading;
using Wrench.DataTypes;

namespace Wrench.Services;

public interface IFlasher
{
    public FlasherResponse Sleep(int timeoutSeconds);
    public FlasherResponse SignalReady();
    public FlasherResponse GetCUReady();
    public FlasherResponse AwaitCUReady(CancellationToken token);
    public FlasherResponse AwaitCUSignal(CancellationToken token);
    public FlasherResponse LockCU();
    public FlasherResponse UnlockCU();
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
    public FlasherResponse AwaitCUState(Func<FlasherResponse> func, CancellationToken token);
    public FlasherResponse GetCUSignal();
    public FlasherResponse GetCURelease();
    public FlasherResponse GetCUState(GpioInputs inputs);
    public FlasherResponse CheckADBDevice();
}

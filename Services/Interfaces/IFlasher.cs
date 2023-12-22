using System;

namespace Wrench.Services;

public interface IFlasher
{
    public void SignalReady();
    public void AwaitCUSignal();
    public void AwaitCUClose();
    public void LockCU();
    public void TurnModemPowerOn();
    public void TurnModemPowerOff();
    public void AwaitDeviceAttach();
    public void AwaitDeviceStart();
    public void TurnOnADBInterface();
    public void TurnOffADBInterface();
    public void ExecuteFastbootBatch();
    public void UpdateCfgSN();
    public void UploadFactoryCFG();
    public void FlasherState();
    public void CheckADBDevice();
}

using System;
using Wrench.Models;
using System.Threading;

namespace Wrench.Services;

public interface IFlasher
{
    public FlasherResponce SignalReady();
    public FlasherResponce GetCUReady();
    public FlasherResponce AwaitCUReady(CancellationToken token);
    public FlasherResponce AwaitCUSignal(CancellationToken token);
    public FlasherResponce LockCU();
    public FlasherResponce TurnModemPowerOn();
    public FlasherResponce TurnModemPowerOff();
    public FlasherResponce AwaitDeviceAttach();
    public FlasherResponce AwaitDeviceStart();
    public FlasherResponce TurnOnADBInterface();
    public FlasherResponce TurnOffADBInterface();
    public FlasherResponce ExecuteFastbootBatch();
    public FlasherResponce UpdateCfgSN();
    public FlasherResponce UploadFactoryCFG();
    public FlasherResponce FlasherState();
    public FlasherResponce CheckADBDevice();
}

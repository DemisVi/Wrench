using System;
using System.Collections.Generic;
using System.Threading;
using Wrench.Models;
using Wrench.DataTypes;

namespace Wrench.Services;

public interface IFlasher : IDisposable
{
    public Package? Package { get; set; }
    public Func<string, int, FlasherResponse> Adb { get; }
    public Func<string, int, FlasherResponse> Fastboot { get; }
    public Func<string, int, Action<string>?, FlasherResponse> SWDConsole { get; }
    public string AdbRebootBootloaderCommand { get; }
    public IEnumerable<KeyValuePair<string, int>> FastbootCommandSequence { get; set; }
    public string FastbootRebootCommand { get; }
    public FlasherResponse Sleep(int timeoutSeconds);
    public FlasherResponse SignalReady();
    public FlasherResponse SignalBusy();
    public FlasherResponse SignalFail();
    public FlasherResponse SignalDone();
    public FlasherResponse GetCUReady();
    public FlasherResponse AwaitCUReady(CancellationToken token);
    public FlasherResponse AwaitCUSignal(CancellationToken token);
    public FlasherResponse AwaitCURelease(CancellationToken token);
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

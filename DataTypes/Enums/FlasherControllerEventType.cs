using System;

namespace Wrench.DataTypes;

public enum FlasherControllerEventType
{
    None,
    ProgressTimerElapsed,
    ProgressTimerReset,
    SuccessState,
    FailState,
    SignalReady,
    SignalBusy,
    LogMessage,
    FlasherStateChanged
}
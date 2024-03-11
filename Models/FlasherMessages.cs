using System;
using Wrench.DataTypes;

public static class FlasherMessages
{
    internal const string Message = "Message";
    internal const string ModemPortNotFound = "Modem Port not found";
    internal const string CancellationRequested = "Cancellation requested";
    internal const string DeviceNotFound = "Device not found";
    internal const string PowerOn = "Board powered on";
    internal const string PowerOff = "Board powered off";
    internal const string LockCU = "Lock Contact Unit: ";
    internal const string UnlockCU = "Unlock Contact Unit: ";
    internal const string CUReady = "Contact Unit ready: ";
    internal const string Done = "Flashing done: ";
    internal const string Busy = "Flashing in progress: ";
    internal const string Fail = "Flashing fail: ";
    internal const string Delay = "Delay: ";
    internal const string DeviceFound = "Device found: ";
    internal const string ContactUnitState = "Contact unit state: ";
    internal const string CantReadAnswer = "Impossible to read answer =(";
    internal const string FactoryUpdated = "'factory.cfg' updated";
    internal const string FactoryDoesntExist = "'factory.cfg' doesn't exist";
    internal const string FileNotFoundFormat = "File {0} doesn't exist";
    internal const string FlashBaseFirmware = "<<< Flash base firmware >>>";
    internal const string UploadFactory = "<<< Upload 'factory.cfg' >>>";
    internal const string EnableADB = "<<< Enable ADB interface >>>";
    internal const string DisableADB = "<<< Disable ADB interface >>>";
    internal const string TimerStart = "Start operation timer...";
    internal const string TimerStop = "Stop operation timer...";
    internal const string TimerReset = "Reset operation timer...";
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wrench.Model
{
    [Flags]
    public enum Sensors : byte
    {
        None = 0,
        Lodg = 1,
        Device = 2,
        Pn1_Up = 4,
        Pn1_Down = 8,
        Pn2_Up = 16,
        Pn2_Down = 32,
    }

    [Flags]
    public enum Outs : byte
    {
        None = 0,
        Green = 1,
        Red = 2,
        Blue = 4,
        Pn1 = 8,
        Pn2 = 16,
        Cyan = Blue | Green,
        Yellow = Green | Red,
        Magenta = Red | Blue,
        White = Red | Green | Blue,
    }

    [Flags]
    public enum ExitCodes
    {
        OK = 0,
        Fail = -1,
        UserTimeout = 1000,
        WasNotRan = 2000,
    }

    [Flags]
    public enum ErrorCodes
    {
        None = 0,
        Lodg = 1,
        Device = 2,
        Pneumo_Up = 4,
        Pneumo_Down = 8,
        Device_Power = 16,
        Device_Attach = 32,
        ADB_Interface = 64,
        Fastboot_Batch = 128,
        ADB_Batch = 256,
        Device_Start = 512,
        Device_AT_config = 1024,
        
    }
}

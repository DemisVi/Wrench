using System;

namespace Wrench.DataTypes;

[Flags]
public enum GpioInputs : byte
{
    None = 0,
    Lodg = 1,
    Device = 2,
    Pn1_Up = 4,
    Pn1_Down = 8,
    Pn2_Up = 16,
    Pn2_Down = 32,
}

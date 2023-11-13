using System;

[Flags]
public enum GpioOutputs : byte
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

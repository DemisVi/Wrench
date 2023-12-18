using System;
using System.Runtime.CompilerServices;
using Wrench.DataTypes;

namespace Wrench.Services;

public class MockCuntactUnit
{
    public GpioOutputs Outputs => GpioOutputs.Pn1 | GpioOutputs.Green;

    public GpioInputs Inputs => GpioInputs.Device;

    public void Cl15Off([CallerMemberName] string name = "", [CallerLineNumber] int line = 0)
    {
        System.Console.WriteLine(name + " " + line);
    }

    public void Cl15On([CallerMemberName] string name = "", [CallerLineNumber] int line = 0)
    {
        System.Console.WriteLine(name + " " + line);
    }

    public void LEDBlue([CallerMemberName] string name = "", [CallerLineNumber] int line = 0)
    {
        System.Console.WriteLine(name + " " + line);
    }

    public void LEDCyan([CallerMemberName] string name = "", [CallerLineNumber] int line = 0)
    {
        System.Console.WriteLine(name + " " + line);
    }

    public void LEDGreen([CallerMemberName] string name = "", [CallerLineNumber] int line = 0)
    {
        System.Console.WriteLine(name + " " + line);
    }

    public void LEDMagenta([CallerMemberName] string name = "", [CallerLineNumber] int line = 0)
    {
        System.Console.WriteLine(name + " " + line);
    }

    public void LEDOff([CallerMemberName] string name = "", [CallerLineNumber] int line = 0)
    {
        System.Console.WriteLine(name + " " + line);
    }

    public void LEDRed([CallerMemberName] string name = "", [CallerLineNumber] int line = 0)
    {
        System.Console.WriteLine(name + " " + line);
    }

    public void LEDWhite([CallerMemberName] string name = "", [CallerLineNumber] int line = 0)
    {
        System.Console.WriteLine(name + " " + line);
    }

    public void LEDYellow([CallerMemberName] string name = "", [CallerLineNumber] int line = 0)
    {
        System.Console.WriteLine(name + " " + line);
    }

    public void LockBoard([CallerMemberName] string name = "", [CallerLineNumber] int line = 0)
    {
        System.Console.WriteLine(name + " " + line);
    }

    public void PowerOffBoard([CallerMemberName] string name = "", [CallerLineNumber] int line = 0)
    {
        System.Console.WriteLine(name + " " + line);
    }

    public void PowerOnBoard([CallerMemberName] string name = "", [CallerLineNumber] int line = 0)
    {
        System.Console.WriteLine(name + " " + line);
    }

    public void ReleaseBoard([CallerMemberName] string name = "", [CallerLineNumber] int line = 0)
    {
        System.Console.WriteLine(name + " " + line);
    }
}

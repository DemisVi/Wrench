using System;
using System.IO.Ports;
using Wrench.Models;

namespace Wrench.Services;

public class ModemPort : SerialPort
{
    public new int BaudRate { get; } = 115200;
    public new string NewLine { get; } = "\r";
    public ModemPort(string portName) : base(portName)
    {
        base.BaudRate = BaudRate;
        base.NewLine = NewLine;
    }

    public static string[] GetModemATPortNames() => SerialPortSearcher.GetPortNames(WqlQueries.ObjectSimcomATPort);
}

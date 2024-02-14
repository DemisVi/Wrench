using System;
using System.Reflection;
using System.IO.Ports;
using System.Linq;
using Microsoft.Win32.SafeHandles;
using Wrench.Models;
using System.Threading;
using System.IO;
using Wrench.DataTypes;

namespace Wrench.Services;

public class ModemPort : SerialPort, IDisposable
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

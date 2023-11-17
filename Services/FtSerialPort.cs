using System;
using System.IO.Ports;
using Wrench.Models;

namespace Wrench.Services;

public class FtSerialPort : SerialPort
{
    public new int BaudRate { get; } = 115200;
    public FtSerialPort(string portName) : base(portName) => base.BaudRate = BaudRate;

    public new static string[] GetPortNames() => SerialPortSearcher.GetPortNames(WqlQueries.ObjectFtdiSerial);
}

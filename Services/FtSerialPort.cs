using System;
using System.Linq;
using System.IO.Ports;
using System.Management;

public class FtSerialPort : SerialPort
{
    public new int BaudRate { get; } = 115200;
    public FtSerialPort(string portName) : base(portName) => base.BaudRate = BaudRate;

    public new static string[] GetPortNames() => SerialPortSearcher.GetPortNames(WqlQueries.ObjectFtdiSerial);
}

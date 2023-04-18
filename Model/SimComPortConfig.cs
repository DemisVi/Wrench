using System;
using System.IO.Ports;

public class SimComPortConfig : IModemPortConfig
{
    public string NewLine { get; init; } = "\r";
    public string PortName { get; } = string.Empty;
    public int BaudRate { get; init; } = 115200;
    public Handshake HandShake { get; init; } = Handshake.None;
    public int ReadTimeout { get; init; } = 1200;
    public int WriteTimeout { get; init; } = 1200;
    public SimComPortConfig(string portName) => PortName = portName;
}

using System;
using System.IO.Ports;

public class TelitPortConfig : IModemPortConfig
{
    public string NewLine { get; init; } = "\r";
    public string PortName { get; } = string.Empty;
    public int BaudRate { get; init; } = 115200;
    public Handshake HandShake { get; init; } = Handshake.RequestToSend;
    public int ReadTimeout { get; init; } = 1200;
    public int WriteTimeout { get; init; } = 1200;
    public TelitPortConfig(string portName) => PortName = portName;
}
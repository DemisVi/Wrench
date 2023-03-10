using System;
using System.IO.Ports;

public interface IModemPortConfig
{
    string NewLine { get; init; }
    string PortName { get; }
    int BaudRate { get; init; }
    Handshake HandShake { get; init; }
    int ReadTimeout { get; init; }
    int WriteTimeout { get; init; }
}

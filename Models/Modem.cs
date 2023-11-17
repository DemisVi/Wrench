using System;
using Wrench.Services;

namespace Wrench.Models;

public class Modem
{
    public string AttachedTo { get; set; } = string.Empty;
    public string SerialNumber { get; set; } = string.Empty;
    public bool IsOnline { get; set; } = default;

    public static string[] GetModemATPortNames() => SerialPortSearcher.GetPortNames(WqlQueries.ObjectSimcomATPort);
}

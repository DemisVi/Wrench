using System;

public class Modem
{
    public string AttachedTo { get; set; } = string.Empty;
    public string SerialNumber { get; set; } = string.Empty;

    public static string[] GetModemATPortNames() => SerialPortSearcher.GetPortNames(WqlQueries.ObjectSimcomATPort);
}

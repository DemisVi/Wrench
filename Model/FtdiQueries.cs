using System;
using System.Management;

 namespace Wrench.Model;

public class FtdiQueries
{
#pragma warning disable CA1416
    public static readonly WqlObjectQuery ftdiSerial = new("SELECT Caption FROM Win32_PnPEntity WHERE Manufacturer = 'FTDI' AND Caption LIKE 'USB Serial Port (%)'");
#pragma warning restore CA1416
}
using System;
using System.Management;

namespace Wrench.Models;

public static class FtdiQueries
{
#pragma warning disable CA1416

    public static WqlObjectQuery FtdiSerialDevice { get; } = new(
        "SELECT Caption FROM Win32_PnPEntity WHERE Manufacturer = 'FTDI' AND Caption LIKE 'USB Serial Port (%)'");

#pragma warning restore CA1416
}

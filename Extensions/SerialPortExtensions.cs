using System.Threading.Tasks;
using System.IO.Ports;
using System.Threading;
using System;

namespace Wrench.Extensions;

public static class SerialExtensions
{
    public static async Task<string> WaitModemStartAsync(this SerialPort port, ModemType modemType)
    {
        if (!port.IsOpen) throw new InvalidOperationException("Modem port closed");

        string res;

        do
        {
            try
            {
                port.WriteLine(modemType.Type);
            }
            catch (TimeoutException) { }
            await Task.Delay(500);
            res = port.ReadExisting();
        } while (!res.Contains("OK"));
        return res;
    }

    public static string WaitModemStart(this SerialPort port, ModemType modemType) => port.WaitModemStartAsync(modemType).GetAwaiter().GetResult();
}
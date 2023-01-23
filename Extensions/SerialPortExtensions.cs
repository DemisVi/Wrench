using System.Threading.Tasks;
using System.IO.Ports;
using System.Threading;
using System;
using System.Linq;

namespace Wrench.Extensions;

public static class SerialExtensions
{
    public static async Task<bool> WaitModemStartAsync(this SerialPort port, ModemType modemType, int timeoutSeconds = 10)
    {
        if (!port.IsOpen) throw new InvalidOperationException("Modem port closed");

        var start = DateTime.Now;
        var elapsed = TimeSpan.Zero;
        string res;

        do
        {
            try
            {
                port.DiscardOutBuffer();
                await Task.Delay(500);
                port.WriteLine(modemType.BootCommand);
            }
            catch (TimeoutException) { }
            res = port.ReadLine();
            res += port.ReadExisting();
            elapsed = DateTime.Now - start;
        } while (!res.Contains("OK") || elapsed.Seconds < timeoutSeconds);
        await Task.Delay(1000);
        port.DiscardInBuffer();
        return res.Contains("OK");
    }

    public static bool WaitModemStart(this SerialPort port, ModemType modemType, int timeoutSeconds = 10) =>
        port.WaitModemStartAsync(modemType, timeoutSeconds).GetAwaiter().GetResult();
}
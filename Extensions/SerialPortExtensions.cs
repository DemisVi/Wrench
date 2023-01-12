using System.Threading.Tasks;
using System.IO.Ports;
using System.Threading;
using System;
using System.Linq;

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
            res = port.ReadLine();
            res += port.ReadExisting();
        } while (!res.Contains("OK"));
        await Task.Delay(2500);
        port.DiscardInBuffer();
        return new string(res.Where(Char.IsDigit).ToArray());
    }

    public static string WaitModemStart(this SerialPort port, ModemType modemType, int timeout = 10) => port.WaitModemStartAsync(modemType).GetAwaiter().GetResult();
}
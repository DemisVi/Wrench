using System.Threading.Tasks;
using System.IO.Ports;
using System.Threading;
using System;

namespace Wrench.Extensions
{
    public static class SerialExtensions
    {
        public static async Task WaitModemStartAsync(this SerialPort port)
        {
            if (!port.IsOpen) throw new InvalidOperationException("Modem port closed");

            string res;

            do
            {
                try
                {
                    port.WriteLine("AT+GSN=1");
                }
                catch (TimeoutException) { }
                await Task.Delay(250);
                res = port.ReadExisting();
            } while (!res.Contains("OK"));
        }

        public static void WaitModemStart(this SerialPort port)
        {
            if (!port.IsOpen) throw new InvalidOperationException("Modem port closed");

            string res;

            do
            {
                try
                {
                    port.WriteLine("AT+GSN=1");
                }
                catch (TimeoutException) { }

                Thread.Sleep(250);
                res = port.ReadExisting();
            } while (!res.Contains("OK"));
        }
    }
}
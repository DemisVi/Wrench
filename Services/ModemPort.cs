using System;
using System.Reflection;
using System.IO.Ports;
using System.Linq;
using Microsoft.Win32.SafeHandles;
using Wrench.Models;
using System.Threading;
using System.IO;
using Wrench.DataTypes;

namespace Wrench.Services;

public class ModemPort : SerialPort, IDisposable
{
    public new int BaudRate { get; } = 115200;
    public new string NewLine { get; } = "\r";
    public ModemPort(string portName) : base(portName)
    {
        base.BaudRate = BaudRate;
        base.NewLine = NewLine;
    }

    public bool TryGetResponce(string line, out string response)
    {
        if (IsOpen is not true) throw new IOException($"Port is not open");

        var handle = (SafeFileHandle?)BaseStream.GetType()
            .GetFields(BindingFlags.NonPublic | BindingFlags.Instance)
            .Where(x => x.Name.Contains("handle"))
            .FirstOrDefault()?
            .GetValue(BaseStream);

        try
        {
            DiscardInBuffer();

            if (this is not null and { IsOpen: true })
                WriteLine(line);
            else
            {
                response = $"Port {nameof(IsOpen)} is {IsOpen}";
                return false;
            }

            Thread.Sleep(200);

            response = ReadExisting().Replace('\r', ' ').Replace('\n', ' ');

            if (string.IsNullOrEmpty(response)) return false;
            else
                return true;
        }
        catch (Exception ex)
        {
            if (handle is not null and { IsClosed: false })
            {
                handle.Close();
                Dispose();
            }

            response = ex.Message;

            return false;
        }
    }

    public static string[] GetModemATPortNames() => SerialPortSearcher.GetPortNames(WqlQueries.ObjectSimcomATPort);
}

using System;
using System.Linq;
using System.Management;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.IO.Ports;
using Wrench.Services;

using Timer = System.Timers.Timer;
using Wrench.Interfaces;

namespace Wrench.Model;

public class ContactUnit2102 : IContactUnit
{
    private static readonly object _lock = new();
    protected const double poolingInterval = 500D;
    protected const int _baseBoudRate = 115200;
    protected const int _readBufferLength = 3;
    protected readonly byte[] _readCommand = new byte[] { 0x33, 0xCC };
    protected readonly byte[] _writeCommand = new byte[] { 0xCC, 0x33 };
    protected SerialPort _serialPort;
    protected TaskCompletionSource<byte> _tcs;

#pragma warning disable CS8618 // singleton
    private static IContactUnit _cu;
#pragma warning restore CS8618 // singleton

    private ContactUnit2102(string serial)
    {
        _serialPort = new(serial, _baseBoudRate);
        _serialPort.Open();
        _tcs = new();
    }

    public static IContactUnit GetInstance(string serial) => _cu ??= new ContactUnit2102(serial);

    public static string GetSerialName()
    {
#pragma warning disable CA1416

        var query = new WqlObjectQuery("SELECT Name FROM Win32_PnPEntity WHERE Caption LIKE '%CP210x USB to UART%'");

        var searcher = new ManagementObjectSearcher(query);
        var text = searcher.Get().Cast<ManagementBaseObject>().First().GetText(TextFormat.Mof);

        var portName = text.Split(new char[] { '(', ')' })
                                   .Where(x => x.Contains("COM", StringComparison.OrdinalIgnoreCase))
                                   .First();

        return portName;

#pragma warning restore CA1416
    }

    public bool PowerOn() => _serialPort.DtrEnable = true;

    public bool PowerOff() => _serialPort.DtrEnable = false;

    internal Sensors GetSensors()
    {
        var result = new byte();
        lock (_lock)
        {
            _tcs = new TaskCompletionSource<byte>();
            _serialPort.DataReceived += ReceiveData;
            _serialPort.Write(_readCommand, 0, _readCommand.Length);

            result = _tcs.Task.Result;

            _serialPort.DataReceived -= ReceiveData;
        }
        return (Sensors)result;
    }

    public Outs SetOuts(Outs outs)
    {
        var result = new byte();
        lock (_lock)
        {
            var request = _writeCommand.Concat(new byte[] { (byte)outs }).ToArray();
            _tcs = new TaskCompletionSource<byte>();
            _serialPort.DataReceived += ReceiveData;
            _serialPort.Write(request, 0, request.Length);

            result = _tcs.Task.Result;

            _serialPort.DataReceived -= ReceiveData;
        }
        return (Outs)result;
    }

    public Sensors WaitForState(Sensors sensors, int timeout = Timeout.Infinite)
    {
        var tcs = new TaskCompletionSource<Sensors>();
        var start = DateTime.Now;
        using var timer = new Timer(poolingInterval)
        {
            Enabled = true,
            AutoReset = true,
        };
        timer.Elapsed += (s, _) =>
        {
            var timer = s as Timer;
            var elapsed = (DateTime.Now - start).TotalSeconds;
            var sens = GetSensors();
            if (sens == sensors)
            {
                if (!_tcs.Task.IsCompleted) tcs.SetResult(sens);
                timer?.Stop();
                return;
            }
            else if (timeout != Timeout.Infinite && elapsed > timeout)
            {
                if (!_tcs.Task.IsCompleted) tcs.SetResult(Sensors.None);
                timer?.Stop();
                return;
            }
        };

        return tcs.Task.Result;
    }

    private void ReceiveData(object? sender, SerialDataReceivedEventArgs args)
    {
        var port = sender as SerialPort;
        var readBuffer = new byte[_readBufferLength];
        port?.Read(readBuffer, 0, _readBufferLength);
        if (!_tcs.Task.IsCompleted) _tcs.SetResult(readBuffer.Last());
    }

    public Sensors WaitForBits(Sensors sensors, int timeout = Timeout.Infinite)
    {
        var tcs = new TaskCompletionSource<Sensors>();
        var start = DateTime.Now;
        using var timer = new Timer(poolingInterval)
        {
            Enabled = true,
            AutoReset = true,
        };
        timer.Elapsed += (s, _) =>
        {
            var timer = s as Timer;
            var elapsed = (DateTime.Now - start).TotalSeconds;
            var sens = GetSensors();
            if ((sens & sensors) == sensors)
            {
                if (!_tcs.Task.IsCompleted) tcs.SetResult(sens);
                timer?.Stop();
                return;
            }
            else if (timeout != Timeout.Infinite && elapsed > timeout)
            {
                if (!_tcs.Task.IsCompleted) tcs.SetResult(Sensors.None);
                timer?.Stop();
                return;
            }
        };

        return tcs.Task.Result;
    }
}
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

public class ContactUnit : IContactUnit
{
    private static readonly object _lock = new();
    protected const double poolingInterval = 500D;
    protected const int _baseBoudRate = 115200;
    protected const string _cuSerialPrefix = "A";
    protected const int _readBufferLength = 3;
    protected readonly byte[] _readCommand = new byte[] { 0x33, 0xCC };
    protected readonly byte[] _writeCommand = new byte[] { 0xCC, 0x33 };
    protected string _ftSerialPort;
    protected Adapter _adapter;
    protected TaskCompletionSource<byte> _tcs;

#pragma warning disable CS8618 // singleton
    private static ContactUnit _cu;
#pragma warning restore CS8618 // singleton

    private ContactUnit(string serial, Action<string>? logMethod = null)
    {
        _adapter = new(serial + _cuSerialPrefix, logMethod);
        _ftSerialPort = GetFtSerialName();
        _tcs = new();
    }

    public static ContactUnit GetInstance(string serial, Action<string>? logMethod = null) => _cu ??= new ContactUnit(serial, logMethod);

    protected string GetFtSerialName()
    {

#pragma warning disable CA1416 // platform warning

        using var searcher = new ManagementObjectSearcher(FtdiQueries.ftdiSerial);
        using var queryResult = searcher.Get();
        var ftdiSerialCollection = new ManagementBaseObject[queryResult.Count];
        searcher.Get().CopyTo(ftdiSerialCollection, 0);
        var text = ftdiSerialCollection.First().GetText(TextFormat.Mof);
        var portName = text.Split(new char[] { '(', ')' })
                           .Where(x => x.Contains("COM", StringComparison.OrdinalIgnoreCase))
                           .First();

#pragma warning restore CA1416 // platform warning

        return portName;
    }

    public bool PowerOn()
    {
        if (!_adapter.OpenAdapter()) return false;
        else if (!_adapter.KL15_On()) return false;
        else if (!_adapter.KL30_On()) return false;
        else if (!_adapter.CloseAdapter()) return false;
        else return true;
    }
    public bool PowerOff()
    {
        if (!_adapter.OpenAdapter()) return false;
        else if (!_adapter.KL15_Off()) return false;
        else if (!_adapter.KL30_Off()) return false;
        else if (!_adapter.CloseAdapter()) return false;
        else return true;
    }

    internal Sensors GetSensors()
    {
        var result = new byte();
        lock (_lock)
        {
            _tcs = new TaskCompletionSource<byte>();
            using var serial = new SerialPort(_ftSerialPort, _baseBoudRate);
            serial.DataReceived += ReceiveData;
            serial.Open();
            serial.Write(_readCommand, 0, _readCommand.Length);

            result = _tcs.Task.Result;

            serial.DataReceived -= ReceiveData;
            serial.Close();
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
            using var serial = new SerialPort(_ftSerialPort, _baseBoudRate);
            serial.DataReceived += ReceiveData;
            serial.Open();
            serial.Write(request, 0, request.Length);

            result = _tcs.Task.Result;

            serial.DataReceived -= ReceiveData;
            serial.Close();
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
            var t = s as Timer;
            var elapsed = (DateTime.Now - start).TotalSeconds;
            var sens = GetSensors();
            if (sens == sensors || (timeout != Timeout.Infinite && elapsed > timeout))
            {
                if (!tcs.Task.IsCompleted)
                    tcs.SetResult(sens);
                t?.Stop();
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
        _tcs.SetResult(readBuffer.Last());
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
                tcs.SetResult(sens);
                timer?.Stop();
                return;
            }
            else if (timeout != Timeout.Infinite && elapsed > timeout)
            {
                tcs.SetResult(Sensors.None);
                timer?.Stop();
                return;
            }
        };

        return tcs.Task.Result;
    }
}
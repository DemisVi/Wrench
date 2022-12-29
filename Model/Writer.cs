using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO.Ports;
using System.Linq;
using System.Management;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Wrench.Extensions;
using Wrench.Services;

namespace Wrench.Model;

internal class Writer : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;
    private readonly ObservableCollection<string> _kuLogList;
    private CancellationTokenSource _cts = new();

    private bool? _isWriterRunning;
    public bool? IsWriterRunning { get => _isWriterRunning; set => ChangeProperty(ref _isWriterRunning, value); }

    private string? _status = null;
    public string? OperationStatus { get => _status; set => ChangeProperty(ref _status, value, nameof(OperationStatus)); }

    private string? _passwordText = null;
    public string? PasswordText { get => _passwordText; set => ChangeProperty(ref _passwordText, value, nameof(PasswordText)); }

    public Writer(ObservableCollection<string> kULogList) => _kuLogList = kULogList;

    private bool ChangeProperty<T>(ref T property, T value, [CallerMemberName] string? propertyName = null)
    {
        if (!Equals(property, value))
        {
            property = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            return true;
        }
        return false;
    }

    public void Start()
    {
        if (_cts.Token.IsCancellationRequested) _cts = new();

        Task.Factory.StartNew(SimComQuery);
    }

    public void Stop()
    {
        _cts.Cancel();
    }

    void SimComQuery()
    {
        // Task starting sequence
        var modemLocator = new ModemLocator(LocatorQuery.queryEventSimcom, LocatorQuery.querySimcomATPort);
        LogMsg(string.Join(' ', modemLocator.WaitDeviceConnect().Cast<ManagementObject>().Select(x => x.GetText(TextFormat.Mof))) + Environment.NewLine);
        var port = modemLocator.GetModemATPorts().First();
        LogMsg(port);
        var serial = new SerialPort(port)
        {
            Handshake = Handshake.RequestToSendXOnXOff,
            NewLine = Environment.NewLine,
        };
        serial.Open();
        LogMsg(serial.WaitModemStart(new SimComModem()));

        LogMsg("started");

        while (true)
        {
            if (_cts.IsCancellationRequested)
            {
                // Task ending sequence
                serial.Close();
                LogMsg("stopped");
                return;
            }
            Thread.Sleep(1000);
        }
    }

    public void LogMsg(string? message) => _kuLogList.Insert(0, message ?? string.Empty);
}

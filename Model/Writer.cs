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
    private const string localNewLine = "\r";
    private const Handshake localHandshake = Handshake.RequestToSend;
    public event PropertyChangedEventHandler? PropertyChanged;
    private readonly ObservableCollection<string> _kuLogList;
    private CancellationTokenSource _cts = new();
    Adapter _adapter;
    SerialPort _modemPort;

    private bool? _isWriterRunning;
    public bool? IsWriterRunning { get => _isWriterRunning; set => SetProperty(ref _isWriterRunning, value); }

    private string? _status = null;
    public string? OperationStatus { get => _status; set => SetProperty(ref _status, value, nameof(OperationStatus)); }

    private string? _passwordText = null;
    public string? PasswordText { get => _passwordText; set => SetProperty(ref _passwordText, value, nameof(PasswordText)); }

    private string? _contactUnit = null;
    public string? ContactUnit { get => _contactUnit; set => SetProperty(ref _contactUnit, value, nameof(ContactUnit)); }

    public Writer(ObservableCollection<string> cULogList)
    {
        _kuLogList = cULogList;
    }

    private bool SetProperty<T>(ref T property, T value, [CallerMemberName] string? propertyName = null)
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
        ContactUnit = new AdapterLocator().AdapterSerials.First() + " типа работа с адаптером";
        // Task starting sequence

        //Wait for CU
        //AwaitCUClose(ContactUnit); //looks done

        // wait for device
        AwaitDeviceAttach(out var portName); //looks done

        //var ser = new SerialPort(portName, 9600)
        //{
        //    NewLine = "\r",
        //};
        //ser.Open();
        //ser.DataReceived += (s, e) => LogMsg((s as SerialPort)?.ReadExisting());
        
        // find modem or AT com port
        AwaitDeviceReady(portName); //looks done

        // turn on adb and reboot
        TurnAdbModeOn(portName);

        // execute fastboot flash sequence / batch flash (with subsequent reboot?)
        //ExecuteFastbootBatch();

        // execute adb upload sequence / batch file upload
        //ExecuteAdbBatch();

        // turn off adb and reboot
        //TurnAdbModeOff();

        LogMsg("started");

        while (true)
        {
            Thread.Sleep(100);
            if (!_cts.IsCancellationRequested) continue;
            // Task ending sequence
            if (_adapter is not null && _adapter is { IsOpen: true })
                _adapter.CloseAdapter();
            if (_modemPort is not null && _modemPort is { IsOpen: true })
                _modemPort.Close();
            LogMsg("stopped");
            break;
        }
    }

    private void TurnAdbModeOn(string portName)
    {
        var serial = new SerialPort(portName)
        {
            Handshake = localHandshake,
            NewLine = localNewLine,
        };
        serial.Open();
        serial.DiscardInBuffer();
        serial.DiscardOutBuffer();
        serial.WriteLine("at+cusbadb=1");
        LogMsg(serial.ReadExisting());
        serial.WriteLine("at+creset");
        LogMsg(serial.ReadExisting());
        Thread.Sleep(1000);
        serial.Close();
    }

    private void AwaitDeviceReady(string portName)
    {
        var serial = new SerialPort(portName)
        {
            Handshake = localHandshake,
            NewLine = localNewLine,
        };
        serial.Open();
        LogMsg(serial.WaitModemStart(new SimComModem()));
        serial.Close();
    }

    private void AwaitDeviceAttach(out string portName)
    {
        var modemLocator = new ModemLocator(LocatorQuery.queryEventSimcom, LocatorQuery.querySimcomModem);
        LogMsg(string.Join(' ', modemLocator.WaitDeviceConnect().Cast<ManagementObject>().Select(x => x.GetText(TextFormat.Mof))) + localNewLine);
        portName = modemLocator.GetModemPortNames().First();
        LogMsg(portName);
    }

    private void AwaitCUClose(string adapterSerial)
    {
        _adapter ??= new Adapter(adapterSerial);
        _adapter.OpenAdapter();
        _adapter.WaitForSensor(true);
        LogMsg("Adapter sensor detected");
        _adapter.CloseAdapter();
        LogMsg("Adapter IsClosed");
    }

    public void LogMsg(string? message) => _kuLogList.Insert(0, message ?? string.Empty);
}

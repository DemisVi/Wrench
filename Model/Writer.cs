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
        if (_cts.IsCancellationRequested) _cts = new();

        Task.Factory.StartNew(SimComQuery);
    }

    public void Stop()
    {
        _cts.Cancel();
    }

    void SimComQuery()
    {
        var counter = 0;
        while (true)
        {
            ContactUnit = new AdapterLocator().AdapterSerials.First() + $" типа {counter++}";
            // Task starting sequence

            //Wait for CU
            //AwaitCUClose(ContactUnit); //looks done

            // wait for device
            LogMsg("Awaiting device attach...");
            var modemPort = AwaitDeviceAttach(); //looks done
            LogMsg($"Modem at {modemPort}");
            // find modem or AT com port
            LogMsg("Awaiting device start...");
            AwaitDeviceReady(modemPort); //looks done

            // turn on adb and reboot
            LogMsg("Turning ADB mode....");
            TurnAdbModeOn(modemPort);

            // wait for device
            LogMsg("Awaiting device attach...");
            modemPort = AwaitDeviceAttach(); //looks done
            LogMsg($"Modem at {modemPort}");

            // find modem or AT com port
            LogMsg("Awaiting device start...");
            AwaitDeviceReady(modemPort); //looks done

            // execute fastboot flash sequence / batch flash (with subsequent reboot?)
            //ExecuteFastbootBatch();

            // execute adb upload sequence / batch file upload
            //ExecuteAdbBatch();

            // turn off adb and reboot
            //TurnAdbModeOff();

            LogMsg("Started");

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

    private bool TurnAdbModeOn(string portName)
    {
        var serial = new SerialPort(portName)
        {
            Handshake = localHandshake,
            NewLine = localNewLine,
        };
        serial.Open();
        serial.WriteLine("at+cusbadb=1");
        if (false == ParseAnswer()) return false;
        serial.WriteLine("at+creset");
        if (false == ParseAnswer()) return false;
        Thread.Sleep(1000);
        serial.Close();

        return true;

        bool ParseAnswer()
        {
            string ans;
            ans = serial.ReadLine();
            ans += serial.ReadExisting();

            return ans.Contains("OK", StringComparison.OrdinalIgnoreCase);
        }
    }

    private void AwaitDeviceReady(string portName)
    {
        var serial = new SerialPort(portName)
        {
            Handshake = localHandshake,
            NewLine = localNewLine,
        };
        serial.Open();
        serial.WaitModemStart(new SimComModem());
        serial.Close();
    }

    private string AwaitDeviceAttach()
    {
        var modemLocator = new ModemLocator(LocatorQuery.queryEventSimcom, LocatorQuery.querySimcomModem);
        //LogMsg(string.Join(' ', modemLocator.WaitDeviceConnect().Cast<ManagementObject>().Select(x => x.GetText(TextFormat.Mof))) + localNewLine);
        modemLocator.WaitDeviceConnect();
        return modemLocator.GetModemPortNames().First();
        //LogMsg(portName);
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

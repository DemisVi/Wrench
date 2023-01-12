using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
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
    private const string fastbootBatch = "flash_most.bat";
    private const string adbBatch = "transfer_to_modem.bat";
    private const Handshake localHandshake = Handshake.RequestToSend;
    public event PropertyChangedEventHandler? PropertyChanged;
    private readonly ObservableCollection<string> _kuLogList;
    private CancellationTokenSource _cts = new();
    Adapter _adapter;
    //SerialPort _modemPort;

    private bool _isWriterRunning = false;
    public bool IsWriterRunning { get => _isWriterRunning; set => SetProperty(ref _isWriterRunning, value); }

    private string _status = string.Empty;
    public string OperationStatus { get => _status; set => SetProperty(ref _status, value, nameof(OperationStatus)); }

    private string _passwordText = string.Empty;
    public string PasswordText { get => _passwordText; set => SetProperty(ref _passwordText, value, nameof(PasswordText)); }

    private string _contactUnit = string.Empty;
    public string ContactUnit { get => _contactUnit; set => SetProperty(ref _contactUnit, value, nameof(ContactUnit)); }

    private string _workingDir = string.Empty;
    public string WorkingDir { get => _workingDir; set => SetProperty(ref _workingDir, value, nameof(WorkingDir)); }

    public Writer(ObservableCollection<string> cULogList)
    {
        _kuLogList = cULogList;
        _adapter = new Adapter(new AdapterLocator().AdapterSerials.First());
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
        ContactUnit = new AdapterLocator().AdapterSerials.First();
        Task.Factory.StartNew(SimComQuery);
    }

    public void Stop()
    {
        _cts.Cancel();
    }

    void SimComQuery()
    {
        // Task starting sequence

        var start = DateTime.Now;

        //Wait for CU
        LogMsg("1. Awaiting CU ready...");
        AwaitCUClose(ContactUnit); //looks done

        ////Turn ON modem power
        //LogMsg("2. Powering board up...");
        //TurnModemPowerOn(ContactUnit);

        // wait for device
        LogMsg("3. Awaiting device attach...");
        var modemPort = AwaitDeviceAttach(); //looks done
        LogMsg($"Modem at {modemPort}");

        // find modem or AT com port
        LogMsg("4. Awaiting device start...");
        AwaitDeviceReady(modemPort); //looks done

        // turn on adb and reboot
        LogMsg("5. Turning ADB mode....");
        LogMsg(TurnAdbModeOn(modemPort).ToString());

        // wait for device
        LogMsg("6. Awaiting device attach...");
        modemPort = AwaitDeviceAttach(); //looks done
        LogMsg($"Modem at {modemPort}");

        //// find modem or AT com port
        //LogMsg("7. Awaiting device start...");
        //AwaitDeviceReady(modemPort); //looks done

        //// execute fastboot flash sequence / batch flash (with subsequent reboot?)
        //LogMsg("8. Fastboot batch...");
        //ExecuteFastbootBatch(WorkingDir); // testing

        // execute adb upload sequence / batch file upload
        ExecuteAdbBatch(WorkingDir);

        // turn off adb and reboot
        //TurnAdbModeOff();

        LogMsg($"Done in {DateTime.Now - start}");

        while (!_cts.IsCancellationRequested)
        {
            Thread.Sleep(1111);
            if (!_cts.IsCancellationRequested) continue;
            // Task ending sequence
            if (_adapter is not null && _adapter is { IsOpen: true })
                _adapter.CloseAdapter();
            //if (_modemPort is not null && _modemPort is { IsOpen: true })
            //    _modemPort.Close();
            LogMsg("Stopped");
            break;
        }
    }

    private void ExecuteAdbBatch(string workingDir)
    {
        if (string.IsNullOrEmpty(workingDir)) throw new ArgumentException($"{nameof(workingDir)} must contain not ampty value");

        var batchFile = Path.Combine(Directory.GetCurrentDirectory(), adbBatch);
        if (!File.Exists(batchFile)) throw new FileNotFoundException("adb batch file not found");

        var dataDir = Path.GetDirectoryName(batchFile)!;

        var batch = new Batch(batchFile, dataDir);

        ParseStdout(batch.Run(), LogMsg);

        bool ParseStdout(string text, Action<string> logMethod)
        {
            var lines = text.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var l in lines) logMethod(l);
            return true;
        }
    }

    private void TurnModemPowerOn(string contactUnit)
    {
        var adapter = new Adapter(contactUnit);
        adapter.OpenAdapter();
        adapter.KL30_On();
        adapter.KL15_On();
        adapter.CloseAdapter();
    }

    private void ExecuteFastbootBatch(string workingDir)
    {
        if (string.IsNullOrEmpty(workingDir)) throw new ArgumentException($"{nameof(workingDir)} must contain not ampty value");

        var batchFile = Path.Combine(Directory.GetCurrentDirectory(), fastbootBatch);
        if (!File.Exists(batchFile)) throw new FileNotFoundException("fastboot batch file not found");

        var systemImage = Directory.EnumerateFiles(workingDir, "system.img", SearchOption.AllDirectories).First();
        if (!File.Exists(systemImage) || string.IsNullOrEmpty(systemImage)) throw new FileNotFoundException("system image not found");

        var systemImageDir = Path.GetDirectoryName(systemImage)!;

        var batch = new Batch(batchFile, systemImageDir);

        ParseStdout(batch.Run(), LogMsg);

        bool ParseStdout(string text, Action<string> logMethod)
        {
            var lines = text.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var l in lines) logMethod(l);
            return true;
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
        Thread.Sleep(1000);
        serial.DiscardOutBuffer();
        serial.WriteLine("at+cusbadb=1");
        if (false == ParseAnswer()) return false;
        serial.WriteLine("at+creset");
        if (false == ParseAnswer()) return false;
        serial.Close();

        return true;

        bool ParseAnswer()
        {
            string ans;
            ans = serial.ReadLine();
            ans += serial.ReadExisting();
            LogMsg(ans);
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
        //serial.WaitModemStart(new TelitModem());
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
        LogMsg($"Adapter IsOpen: {_adapter.IsOpen}");
    }

    public void LogMsg(string? message) => _kuLogList.Insert(0, message ?? string.Empty);
}

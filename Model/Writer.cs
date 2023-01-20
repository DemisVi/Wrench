using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Management;
using System.Reflection;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;
using Wrench.Extensions;
using Wrench.Services;

namespace Wrench.Model;

internal class Writer : INotifyPropertyChanged
{
    private const string localNewLine = "\r";
    private const Handshake localHandshake = Handshake.RequestToSend;
    private const string fastbootBatch = "flash_most.bat";
    private const string adbBatch = "transfer_to_modem.bat";
    public event PropertyChangedEventHandler? PropertyChanged;
    private readonly ObservableCollection<string> _kuLogList;
    private CancellationTokenSource _cts = new();
    ContactUnit _cu;
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

    private Brush _logBgColor = Brushes.White;
    public Brush LogBgColor { get => _logBgColor; set => SetProperty(ref _logBgColor, value); }

    public Writer(ObservableCollection<string> cULogList)
    {
        _kuLogList = cULogList;
        _cu = Wrench.Model.ContactUnit.GetInstance(new AdapterLocator().AdapterSerials.First().Trim('A'));
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
        ContactUnit = new AdapterLocator().AdapterSerials.First().Trim('A');
        Task.Factory.StartNew(SimComQuery);
    }

    public void Stop()
    {
        _cts.Cancel();
    }

    private void SimComQuery()
    {
        // Task starting sequence
        var modemPort = string.Empty;
        DateTime start;
        object opResult;

        if (!DiagnoseCU())
        {
            LogMsg("Contact Unit fialure!");
            Stop();
        }

        while (!_cts.IsCancellationRequested)
        {
            start = DateTime.Now;

            SignalReady();

            //Wait for CU
            LogMsg("Awaiting CU ready...");
            opResult = AwaitCUClose();
            if (opResult as Sensors? is not (Sensors.Lodg | Sensors.Device | Sensors.Pn1_Down))
            {
                LogMsg("Contact Unit fault");
                WriterFaultState();
                continue;
            }
            LogMsg($"{nameof(AwaitCUClose)} returned {opResult}"); //looks done

            if (!LockCU())
            {
                LogMsg("Failed to lock Contact Unit");
                WriterFaultState();
                continue;
            }

            //Turn ON modem power
            LogMsg("Powering board up...");
            opResult = TurnModemPowerOn();
            if (opResult is not true)
            {
                LogMsg("Failed to power on board");
                WriterFaultState();
                continue;
            }
            LogMsg($"{nameof(TurnModemPowerOn)} returned {opResult}"); //looks done

            // wait for device
            LogMsg("Awaiting device attach...");
            try
            {
                modemPort = AwaitDeviceAttach(); //looks done
            }
            catch (Exception)
            {
                WriterFaultState();
                continue;
            }
            LogMsg($"Modem at {modemPort}");

            // find modem or AT com port
            LogMsg("Awaiting device start...");
            opResult = AwaitDeviceReady(modemPort, 10);
            if (opResult is not true)
            {
                LogMsg("Device dows not start within expected interval");
                WriterFaultState();
                continue;
            }
            LogMsg($"{nameof(AwaitDeviceReady)} returned {opResult}"); //looks done

            // turn on adb and reboot
            LogMsg("Reboot for ADB mode....");
            opResult = RebootForAdb(modemPort);
            if (opResult is not true)
            {
                LogMsg("Failed to reboot device");
                WriterFaultState();
                continue;
            }
            LogMsg($"{nameof(RebootForAdb)} returned {opResult}"); //looks done

            // wait for device
            //LogMsg("6. Awaiting device attach...");
            //modemPort = AwaitDeviceAttach(); //looks done
            //LogMsg($"Modem at {modemPort}");

            //// find modem or AT com port
            //LogMsg("7. Awaiting device start...");
            //AwaitDeviceReady(modemPort); //looks done

            // execute fastboot flash sequence / batch flash (with subsequent reboot?)
            LogMsg("Fastboot batch...");
            LogMsg($"{nameof(ExecuteFastbootBatch)} returned {ExecuteFastbootBatch(WorkingDir)}"); // testing

            // wait for device
            LogMsg("Awaiting device attach...");
            try
            {
                modemPort = AwaitDeviceAttach(); //looks done
            }
            catch (Exception)
            {
                WriterFaultState();
                continue;
            }
            LogMsg($"Modem at {modemPort}");

            // find modem or AT com port
            LogMsg("Awaiting device start...");
            opResult = AwaitDeviceReady(modemPort, 10);
            if (opResult is not true)
            {
                LogMsg("Device dows not start within expected interval");
                WriterFaultState();
                continue;
            }
            LogMsg($"{nameof(AwaitDeviceReady)} returned {opResult}"); //looks done

            // turn on adb and reboot
            LogMsg("Reboot for ADB mode....");
            LogMsg($"{nameof(RebootForAdb)} returned {RebootForAdb(modemPort)}");

            // execute adb upload sequence / batch file upload
            LogMsg("Adb batch...");
            LogMsg($"{nameof(ExecuteAdbBatch)} returned {ExecuteAdbBatch(WorkingDir)}");

            // turn off adb and reboot / option: finalizing AT sequence
            //TurnAdbModeOff();

            LogMsg($"Done in {DateTime.Now - start}");


            if (!_cts.IsCancellationRequested) continue;

            LogBgColor = Brushes.White;

            //if (_cu is not null && _cu is { IsOpen: true })
            //    _cu.CloseAdapter();
            //if (_modemPort is not null && _modemPort is { IsOpen: true })
            //    _modemPort.Close();
            LogMsg("Stopped");
            break;
        }
    }

    private bool LockCU()
    {
        _cu.SetOuts(Outs.Pn1 | Outs.Blue);
        return _cu.WaitForState(Sensors.Lodg | Sensors.Device | Sensors.Pn1_Up, 2) != Sensors.None;
    }

    private void SignalReady() => _cu.SetOuts(Outs.Yellow);

    private bool DiagnoseCU(int timeout = 2)
    {
        var lastOuts = _cu.SetOuts(Outs.White | Outs.Pn1);
        _cu.WaitForBits(Sensors.Pn1_Up, timeout);
        _cu.SetOuts(lastOuts ^ Outs.Pn1);
        var lastSensors = _cu.WaitForBits(Sensors.Pn1_Down, timeout);
        return (lastSensors & Sensors.Pn1_Down) != Sensors.None;
    }

    private bool ExecuteAdbBatch(string workingDir)
    {
        if (string.IsNullOrEmpty(workingDir)) throw new ArgumentException($"{nameof(workingDir)} must contain not ampty value");

        var batchFile = Path.Combine(workingDir, adbBatch);
        if (!File.Exists(batchFile)) throw new FileNotFoundException("adb batch file not found");

        var dataDir = Path.GetDirectoryName(batchFile)!;

        var batch = new Batch(batchFile, dataDir);

        batch.Run();

        return batch.ExitCode == 0;
    }

    private bool ExecuteFastbootBatch(string workingDir)
    {
        if (string.IsNullOrEmpty(workingDir)) throw new ArgumentException($"{nameof(workingDir)} must contain not ampty value");

        var batchFile = Path.Combine(Directory.GetCurrentDirectory(), fastbootBatch);
        if (!File.Exists(batchFile)) throw new FileNotFoundException("fastboot batch file not found");

        var systemImage = Directory.EnumerateFiles(workingDir, "system.img", SearchOption.AllDirectories).First();
        if (!File.Exists(systemImage) || string.IsNullOrEmpty(systemImage)) throw new FileNotFoundException("system image not found");

        var systemImageDir = Path.GetDirectoryName(systemImage)!;

        var batch = new Batch(batchFile, systemImageDir);

        batch.Run();

        return batch.ExitCode == 0;
    }

    private bool RebootForAdb(string portName)
    {
        var serial = new SerialPort(portName)
        {
            Handshake = localHandshake,
            NewLine = localNewLine,
        };
        serial.Open();
        Thread.Sleep(1000);
        serial.DiscardOutBuffer();
        serial.DiscardInBuffer();
        serial.WriteLine("at+creset");
        var res = ParseAnswer();
        serial.Close();

        return res;

        bool ParseAnswer()
        {
            string ans;
            ans = serial.ReadLine();
            ans += serial.ReadExisting();
            return ans.Contains("OK", StringComparison.OrdinalIgnoreCase);
        }
    }

    private bool AwaitDeviceReady(string portName, int timeout)
    {
        var serial = new SerialPort(portName)
        {
            Handshake = localHandshake,
            NewLine = localNewLine,
        };
        serial.Open();
        var res = serial.WaitModemStart(new SimComADB(), timeout);
        serial.Close();
        return res;
    }

    private string AwaitDeviceAttach()
    {
        var modemLocator = new ModemLocator(LocatorQuery.queryEventSimcom, LocatorQuery.querySimcomModem);
        try { modemLocator.WaitDeviceConnect(new TimeSpan(0, 0, 30)); }
        catch (Exception)
        {
            LogMsg("Device does not appear within expected interval");
            throw;
        }
        return modemLocator.GetModemPortNames().First();
    }

    private void WriterFaultState()
    {
        _cu.SetOuts(Outs.Red);
        _cu.WaitForState(Sensors.Pn1_Down);
    }

    private bool TurnModemPowerOn() => _cu.PowerOn();

    private Sensors AwaitCUClose() => _cu.WaitForState(Sensors.Lodg | Sensors.Device | Sensors.Pn1_Down);

    public void LogMsg(string? message) => _kuLogList.Insert(0, message ?? string.Empty);
}

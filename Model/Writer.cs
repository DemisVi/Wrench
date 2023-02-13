using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
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
using System.Timers;
using System.Windows.Media;
using Wrench.Extensions;
using Wrench.Services;

namespace Wrench.Model;

using Timer = System.Timers.Timer;

internal class Writer : INotifyPropertyChanged
{
    private const string localNewLine = "\r";
    private const Handshake localHandshake = Handshake.None;
    private const string fastbootBatch = "flash_most.bat";
    private const string adbBatch = "load_cfg.bat";
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

    private Brush _statusColor = Brushes.White;
    public Brush StatusColor { get => _statusColor; set => SetProperty(ref _statusColor, value); }

    private int _passValue;
    public int PassValue { get => _passValue; set => SetProperty(ref _passValue, value); }

    private int _failValue;
    public int FailValue { get => _failValue; set => SetProperty(ref _failValue, value); }

    private int _progressValue = 0;
    public int ProgressValue { get => _progressValue; set => SetProperty(ref _progressValue, value); }

    private bool _progressIndeterminate = false;
    public bool ProgressIndeterminate { get => _progressIndeterminate; set => SetProperty(ref _progressIndeterminate, value); }

    private TimeSpan _timeAvgValue;
    public TimeSpan TimeAvgValue { get => _timeAvgValue; set => SetProperty(ref _timeAvgValue, value); }

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
        Task.Factory.StartNew(SimcomFlash);
    }

    public void Stop()
    {
        _cts.Cancel();
    }

    private void SimcomFlash()
    {
        // Task starting sequence
        var modemPort = string.Empty;
        DateTime start;
        TimeSpan elapsed = TimeSpan.Zero;
        object opResult;

#if DEBUG
        LogMsg("DiagnoseCU();");
#elif RELEASE
        ProgressIndeterminate = true;
        LogMsg("Close Contact Unit!");
        if (!DiagnoseCU(60))
        {
            LogMsg("Contact Unit fialure!");
            StatusColor = Brushes.LightPink;
            _cu.SetOuts(Outs.Red);
            Stop();
        }
        ProgressIndeterminate = false;
#endif

        while (!_cts.IsCancellationRequested)
        {
            start = DateTime.Now;
#if DEBUG

#elif RELEASE
            SignalReady();

            UpdateCfgSN();

            if (_cts.IsCancellationRequested)
            {
                LogMsg("Stopped");
                WriterStopState();
                break;

            }

            // 1. Wait for CU
            LogMsg("Awaiting CU ready...");
            ProgressIndeterminate = true;
            opResult = AwaitCUClose();
            ProgressIndeterminate = false;
            if (opResult as Sensors? is not (Sensors.Lodg | Sensors.Device | Sensors.Pn1_Down))
            {
                LogMsg("Contact Unit fault");
                WriterFaultState();
                continue;
            }
            LogMsg($"{nameof(AwaitCUClose)} returned {opResult}"); //looks done

            if (_cts.IsCancellationRequested)
            {
                LogMsg("Stopped");
                WriterStopState();
                break;

            }

            if (!LockCU())
            {
                LogMsg("Failed to lock Contact Unit");
                WriterFaultState();
                continue;
            }

            if (_cts.IsCancellationRequested)
            {
                LogMsg("Stopped");
                WriterStopState();
                break;

            }

            // 2. Turn ON modem power
            ProgressValue = 10;
            LogMsg("Powering board up...");
            opResult = TurnModemPowerOn();
            if (opResult is not true)
            {
                LogMsg("Failed to power on board");
                WriterFaultState();
                continue;
            }
            LogMsg($"{nameof(TurnModemPowerOn)} returned {opResult}"); //looks done

            if (_cts.IsCancellationRequested)
            {
                LogMsg("Stopped");
                WriterStopState();
                break;

            }
#endif
            // 3. wait for device
            ProgressValue = 20;
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
            LogMsg($"{nameof(AwaitDeviceAttach)} returned 'Modem at {modemPort}'");

            if (_cts.IsCancellationRequested)
            {
                LogMsg("Stopped");
                WriterStopState();
                break;

            }

            // 4. find modem or AT com port
            ProgressValue = 30;
            LogMsg("Awaiting device start...");
            opResult = AwaitDeviceStart(modemPort);
            if (opResult is not true)
            {
                LogMsg("Device dows not start within expected interval");
                WriterFaultState();
                continue;
            }
            LogMsg($"{nameof(AwaitDeviceStart)} returned {opResult}"); //looks done

            if (_cts.IsCancellationRequested)
            {
                LogMsg("Stopped");
                WriterStopState();
                break;

            }

            // 5. turn on adb and reboot
            ProgressValue = 40;
            LogMsg("Reboot for ADB mode....");
            opResult = RebootForAdb(modemPort);
            if (opResult is not true)
            {
                LogMsg("Failed to reboot device");
                WriterFaultState();
                continue;
            }
            LogMsg($"{nameof(RebootForAdb)} returned {opResult}"); //looks done

            if (_cts.IsCancellationRequested)
            {
                LogMsg("Stopped");
                WriterStopState();
                break;

            }
#if DEBUG
            LogMsg("ExecuteFastbootBatch(WorkingDir);");
#elif RELEASE
            // 6. execute fastboot flash sequence / batch flash (with subsequent reboot?)
            ProgressValue = 50;
            LogMsg("Fastboot batch...");
            opResult = ExecuteFastbootBatch(WorkingDir);
            if (opResult is not true)
            {
                LogMsg("Failed to run Fastboot");
                WriterFaultState();
                continue;
            }
            LogMsg($"{nameof(ExecuteFastbootBatch)} returned {opResult}"); // testing

            if (_cts.IsCancellationRequested)
            {
                LogMsg("Stopped");
                WriterStopState();
                break;

            }
#endif
            // 7. wait for device
            ProgressValue = 60;
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
            LogMsg($"{nameof(AwaitDeviceAttach)} returned 'Modem at {modemPort}'");

            if (_cts.IsCancellationRequested)
            {
                LogMsg("Stopped");
                WriterStopState();
                break;

            }

            // 8. find modem or AT com port
            ProgressValue = 70;
            LogMsg("Awaiting device start...");
            opResult = AwaitDeviceStart(modemPort);
            if (opResult is not true)
            {
                LogMsg("Device dows not start within expected interval");
                WriterFaultState();
                continue;
            }
            LogMsg($"{nameof(AwaitDeviceStart)} returned {opResult}"); //looks done

            if (_cts.IsCancellationRequested)
            {
                LogMsg("Stopped");
                WriterStopState();
                break;

            }

            // 9. turn on adb and reboot
            ProgressValue = 80;
            LogMsg("Reboot for ADB mode....");
            opResult = RebootForAdb(modemPort);
            if (opResult is not true)
            {
                LogMsg("Failed to reboot device");
                WriterFaultState();
                continue;
            }
            LogMsg($"{nameof(RebootForAdb)} returned {opResult}"); //looks done

            if (_cts.IsCancellationRequested)
            {
                LogMsg("Stopped");
                WriterStopState();
                break;

            }

            // 10. execute adb upload sequence / batch file upload
            ProgressValue = 90;
            LogMsg("Adb batch...");
            opResult = ExecuteAdbBatch(WorkingDir);
            if (opResult is not true)
            {
                LogMsg("Failed to run ADB");
                WriterFaultState();
                continue;
            }
            LogMsg($"{nameof(ExecuteAdbBatch)} returned {opResult}");

            if (_cts.IsCancellationRequested)
            {
                LogMsg("Stopped");
                WriterStopState();
                break;

            }
#if DEBUG

#elif RELEASE
            // 2. Turn OFF modem power
            LogMsg("Powering board down...");
            opResult = TurnModemPowerOff();
            if (opResult is not true)
                LogMsg("Failed to power off board");
            LogMsg($"{nameof(TurnModemPowerOff)} returned {opResult}"); //looks done

            // turn off adb and reboot / option: finalizing AT sequence
            //TurnAdbModeOff();
#endif
            elapsed = DateTime.Now - start;

            LogMsg($"Done in {elapsed}");

            WriterSuccessState(elapsed);

            if (!_cts.IsCancellationRequested) continue;

            StatusColor = Brushes.White;

            //if (_cu is not null && _cu is { IsOpen: true })
            //    _cu.CloseAdapter();
            //if (_modemPort is not null && _modemPort is { IsOpen: true })
            //    _modemPort.Close();
            LogMsg("Stopped");
            break;
        }
    }

    private void UpdateCfgSN()
    {
        var dir = Path.GetDirectoryName(WorkingDir);
        var factory = new FactoryCFG(dir);
        factory.UpdateFactory();
    }

    private bool LockCU()
    {
        _cu.SetOuts(Outs.Pn1 | Outs.Blue);
        StatusColor = Brushes.LightBlue;
        return _cu.WaitForState(Sensors.Lodg | Sensors.Device | Sensors.Pn1_Up, 2) != Sensors.None;
    }

    private void SignalReady()
    {
        _cu.SetOuts(Outs.Yellow);
        StatusColor = Brushes.Wheat;
    }

    private bool DiagnoseCU(int timeout = Timeout.Infinite)
    {
        StatusColor = Brushes.White;
        if (_cu.WaitForBits(Sensors.Lodg, timeout) == Sensors.None) return false;
        var lastOuts = _cu.SetOuts(Outs.White | Outs.Pn1);
        _cu.WaitForBits(Sensors.Pn1_Up | Sensors.Lodg, timeout);
        _cu.SetOuts(lastOuts ^ Outs.Pn1);
        var lastSensors = _cu.WaitForBits(Sensors.Pn1_Down, timeout);
        return (lastSensors & Sensors.Pn1_Down) != Sensors.None;
    }

    private bool ExecuteAdbBatch(string workingDir)
    {
        if (string.IsNullOrEmpty(workingDir)) throw new ArgumentException($"{nameof(workingDir)} must contain not ampty value");

        var batchFile = Path.Combine(Directory.GetCurrentDirectory(), adbBatch);
        if (!File.Exists(batchFile)) throw new FileNotFoundException("adb batch file not found");

        var dataDir = Path.GetDirectoryName(workingDir);

        var batch = new Batch(batchFile, dataDir!);

        batch.Run();

        return batch.ExitCode == ExitCodes.OK;
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

        return batch.ExitCode == ExitCodes.OK;
    }
    internal bool RebootForAdb(string portName, int timeout = 30)
    {
        var tcs = new TaskCompletionSource<bool>();
        var start = DateTime.Now;

        using var timer = new Timer(1000D)
        {
            Enabled = true,
            AutoReset = true,
        };

        var serial = new SerialPort(portName)
        {
            Handshake = localHandshake,
            NewLine = localNewLine,
        };

        serial.DataReceived += DataReceived;
        serial.Open();

        timer.Elapsed += (s, _) =>
        {
            var timer = s as Timer;
            var elapsed = (DateTime.Now - start).TotalSeconds;
            serial.WriteLine("at+creset");

            if (timeout != Timeout.Infinite && elapsed > timeout)
            {
                if (tcs.Task is { IsCompleted: true }) return;
                tcs.SetResult(false);
            }
        };

        var res = tcs.Task.Result;

        timer.Stop();
        serial.Close();
        serial.DataReceived -= DataReceived;

        return res;

        void DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            var serial = sender as SerialPort;
            var buffer = serial?.ReadExisting();
            if (buffer is not null && buffer.Contains("OK"))
            {
                if (tcs.Task is { IsCompleted: true }) return;
                tcs.SetResult(true);
            }
        }
    }

    private bool AwaitDeviceStart(string portName, int timeout = Timeout.Infinite)
    {
        var tcs = new TaskCompletionSource<bool>();
        var command = new SimComADB().BootCommand;
        var elapsed = 0;
        var serial = new SerialPort(portName)
        {
            Handshake = localHandshake,
            NewLine = localNewLine,
        };

        var timer = new Timer(1000D)
        {
            Enabled = true,
            AutoReset = true,
        };

        timer.Elapsed += (s, _) =>
        {
            elapsed++;
            serial.WriteLine(command);
        };

        serial.DataReceived += DataReceived;
        serial.Open();

        var res = tcs.Task.Result;

        serial.DataReceived -= DataReceived;
        serial.Close();
        timer.Stop();

        return res;

        void DataReceived(object s, SerialDataReceivedEventArgs e)
        {
            if (tcs.Task is { IsCompleted: true }) return;
            var port = s as SerialPort;
            var data = port?.ReadExisting();
            if (data is not null && data.Contains("OK"))
            {
                if (tcs.Task is { IsCompleted: true }) return;
                tcs.SetResult(true);
            }
            else if (timeout != Timeout.Infinite && elapsed > timeout)
            {
                if (tcs.Task is { IsCompleted: true }) return;
                tcs.SetResult(false);
            }
        }
    }

    private string AwaitDeviceAttach()
    {
        var modemLocator = new ModemLocator(LocatorQuery.queryEventSimcom, LocatorQuery.querySimcomATPort);
        try { modemLocator.WaitDeviceAttach(new TimeSpan(0, 0, 60)); }
        catch (Exception)
        {
            LogMsg("Device does not appear within expected interval");
            throw;
        }
        return modemLocator.GetModemATPorts().First();
    }

    private void WriterFaultState()
    {
        ProgressIndeterminate = true;
        _cu.SetOuts(Outs.Red);
        StatusColor = Brushes.LightPink;
        FailValue++;
#if DEBUG

        Thread.Sleep(new TimeSpan(0, 0, 5));
#elif RELEASE

        _cu.WaitForState(Sensors.Pn1_Down);
#endif

        ProgressIndeterminate = false;
    }

    private void WriterStopState()
    {
        ProgressValue = 0;
        _cu.SetOuts(Outs.White);
        StatusColor = Brushes.White;
#if DEBUG

        Thread.Sleep(new TimeSpan(0, 0, 5));
#elif RELEASE

        _cu.WaitForState(Sensors.Pn1_Down);
#endif

    }

    private void WriterSuccessState(TimeSpan elapsed)
    {
        ProgressValue = 100;
        _cu.SetOuts(Outs.Green);
        StatusColor = Brushes.LightGreen;
        PassValue++;
        TimeAvgValue = elapsed;
#if DEBUG

        Thread.Sleep(new TimeSpan(0, 0, 5));
#elif RELEASE

        _cu.WaitForState(Sensors.Pn1_Down);
#endif

        ProgressValue = 0;
    }

    private bool TurnModemPowerOn() => _cu.PowerOn();

    private bool TurnModemPowerOff(int timeout = 2)
    {
        Thread.Sleep(new TimeSpan(0, 0, timeout));

        _cu.PowerOff();

        return true;
    }

    private Sensors AwaitCUClose() => _cu.WaitForState(Sensors.Lodg | Sensors.Device | Sensors.Pn1_Down);

    public void LogMsg(string? message) => _kuLogList.Insert(0, message ?? string.Empty);
}

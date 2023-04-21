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
using Wrench.Interfaces;
using Wrench.Services;

namespace Wrench.Model;

using Timer = System.Timers.Timer;

internal class TelitWriter : INotifyPropertyChanged, IWriter
{
    private const string localNewLine = "\r";
    private const Handshake localHandshake = Handshake.None;
    private const string adbBatch = "transfer_to_modem.bat";
    private const string atCommandFileName = "postinstallat.txt";
    public event PropertyChangedEventHandler? PropertyChanged;
    private readonly ObservableCollection<string> _kuLogList;
    private CancellationTokenSource _cts = new();
    ContactUnit _cu;

    private bool _isWriterRunning = false;
    public bool IsWriterRunning { get => _isWriterRunning; set => SetProperty(ref _isWriterRunning, value); }

    private string _status = string.Empty;
    public string OperationStatus { get => _status; set => SetProperty(ref _status, value, nameof(OperationStatus)); }

    private string _contactUnit = string.Empty;
    public string ContactUnitTitle { get => _contactUnit; set => SetProperty(ref _contactUnit, value, nameof(ContactUnitTitle)); }

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

    private string _deviceSerial = string.Empty;
    public string DeviceSerial { get => _deviceSerial; set => SetProperty(ref _deviceSerial, value); }

    private TimeSpan _timeAvgValue;
    public TimeSpan TimeAvgValue { get => _timeAvgValue; set => SetProperty(ref _timeAvgValue, value); }

    public TelitWriter(ObservableCollection<string> cULogList)
    {
        _kuLogList = cULogList;
        _cu = Wrench.Model.ContactUnit.GetInstance(new AdapterLocator().AdapterSerials.First().Trim(new[] { 'A', 'B' }));
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
        ContactUnitTitle = new AdapterLocator().AdapterSerials.First().Trim(new[] { 'A', 'B' });
        Task.Factory.StartNew(TelitFlash);
    }

    public void Stop()
    {
        _cts.Cancel();
    }

    private void TelitFlash()
    {
        // Task starting sequence
        var modemPort = string.Empty;
        DateTime start;
        TimeSpan elapsed = TimeSpan.Zero;
        object opResult, expected;

        //ProgressIndeterminate = true;
        //LogMsg("Close Contact Unit!");
        //if (!DiagnoseCU(60))
        //{
        //    LogMsg("Contact Unit fialure!");
        //    StatusColor = Brushes.LightPink;
        //    _cu.SetOuts(Outs.Red);
        //    Stop();
        //}
        //ProgressIndeterminate = false;

        while (!_cts.IsCancellationRequested)
        {
            SignalReady();
            ProgressValue = 0;

            if (_cts.IsCancellationRequested)
            {
                LogMsg("Остановлено");
                WriterStopState();
                break;
            }

            // 1. Wait for CU
            LogMsg("Ожидание готовности КУ...");
            ProgressIndeterminate = true;
            opResult = AwaitCUClose();
            expected = Sensors.Lodg | Sensors.Device | Sensors.Pn1_Down;
            ProgressIndeterminate = false;
            if (opResult as Sensors? != (expected as Sensors?))
            {
                LogMsg($"ERROR: {((int)opResult ^ (int)expected):D4} \nContact Unit fault");
                WriterFaultState();
                continue;
            }

            if (_cts.IsCancellationRequested)
            {
                LogMsg("Остановлено");
                WriterStopState();
                break;

            }

            start = DateTime.Now;
            opResult = LockCU();
            expected = Sensors.Lodg | Sensors.Device | Sensors.Pn1_Up;
            if (opResult as Sensors? != expected as Sensors?)
            {
                LogMsg($"ERROR: {((int)opResult ^ (int)expected):D4} \nFailed to lock Contact Unit");
                WriterFaultState();
                continue;
            }

            if (_cts.IsCancellationRequested)
            {
                LogMsg("Остановлено");
                WriterStopState();
                break;
            }

            // 2. Turn ON modem power
            ProgressValue = 10;
            LogMsg("Подача питания...");
            opResult = TurnModemPowerOn();
            if (opResult is not true)
            {
                LogMsg($"ERROR: {(int)ErrorCodes.Device_Power:D4} \nFailed to power on board");
                WriterFaultState();
                continue;
            }

            if (_cts.IsCancellationRequested)
            {
                LogMsg("Остановлено");
                WriterStopState();
                break;
            }
            
            // 3. wait for device
            ProgressValue = 20;
            LogMsg("Ожидание подключения...");
            try
            {
                modemPort = AwaitDeviceAttach(); //looks done
            }
            catch (Exception)
            {
                LogMsg($"ERROR: {(int)ErrorCodes.Device_Attach:D4} \nDevice does not appear within expected interval");
                WriterFaultState();
                continue;
            }

            if (_cts.IsCancellationRequested)
            {
                LogMsg("Остановлено");
                WriterStopState();
                break;

            }

            // Reflash modem
            LogMsg("Запуск TFI программатора...");
            opResult = ModemReflasher.TryRestoreFirmware(LogMsg, WorkingDir);
            if (opResult is not true)
            {
                LogMsg($"ERROR: {(int)ErrorCodes.Fastboot_Batch:D4} \nОшибка прошивки по средствам TFI");
                WriterFaultState();
                continue;
            }

            ProgressValue = 60;

            if (_cts.IsCancellationRequested)
            {
                LogMsg("Остановлено");
                WriterStopState();
                break;
            }
            
            // 3. wait for device
            LogMsg("Ожидание подключения...");
            try
            {
                modemPort = AwaitDeviceAttach(); //looks done
            }
            catch (Exception)
            {
                LogMsg($"ERROR: {(int)ErrorCodes.Device_Attach:D4} \nDevice does not appear within expected interval");
                WriterFaultState();
                continue;
            }

            if (_cts.IsCancellationRequested)
            {
                LogMsg("Остановлено");
                WriterStopState();
                break;

            }

            // 8. find modem or AT com port
            ProgressValue = 70;
            LogMsg("Ожидание запуска...");
            opResult = AwaitDevice(modemPort, new TelitModem().BootCommand, 30);
            if (opResult is not true)
            {
                LogMsg($"ERROR: {(int)ErrorCodes.Device_Start:D4} \nDevice dows not start within expected interval");
                WriterFaultState();
                continue;
            }

            if (_cts.IsCancellationRequested)
            {
                LogMsg("Остановлено");
                WriterStopState();
                break;
            }

            //10. execute adb upload sequence / batch file upload
            LogMsg("Загрузка файлов через ADB интерфейс...");
            opResult = ExecuteAdbBatch(WorkingDir);
            if (opResult is not true)
            {
                LogMsg($"ERROR: {(int)ErrorCodes.ADB_Batch:D4} \nFailed to run ADB");
                WriterFaultState();
                continue;
            }

            if (_cts.IsCancellationRequested)
            {
                LogMsg("Остановлено");
                WriterStopState();
                break;
            }

            LogMsg("Отправка конфигурационных АТ команд...");
            opResult = SendATCommands(modemPort, Path.Combine(WorkingDir, atCommandFileName));
            if (opResult is not true)
            {
                LogMsg($"ERROR: {(int)ErrorCodes.Device_AT_config:D4} \nFailed to sent AT commands");
                WriterFaultState();
                continue;
            }

            if (_cts.IsCancellationRequested)
            {
                LogMsg("Остановлено");
                WriterStopState();
                break;
            }

            // 2. Turn OFF modem power
            LogMsg("Снятие питания...");
            opResult = TurnModemPowerOff();
            if (opResult is not true)
                LogMsg($"ERROR: {(int)ErrorCodes.Device_Power:D4} \nFailed to power off board");

            elapsed = DateTime.Now - start;

            LogMsg($"Завершено за {elapsed}");

            WriterSuccessState(elapsed);

            if (!_cts.IsCancellationRequested) continue;

            StatusColor = Brushes.White;

            LogMsg("Остановлено");
            break;
        }
    }

    private bool SendATCommands(string modemPort, string commandFilePath)
    {
        var writer = new ATWriter(new TelitPortConfig(modemPort), commandFilePath);

        try
        {
            return writer.SendCommands();
        }
        catch (Exception)
        {
            throw;
        }
    }

    private bool TurnOnADBInterface(string modemPort)
    {
        using var cts = new CancellationTokenSource();
        var task = Task.Factory.StartNew(() =>
        {
            var repeat = 3;
            using var modemSerialPort = new SerialPort()
            {
                Handshake = localHandshake,
                NewLine = localNewLine,
                PortName = modemPort,
                WriteTimeout = 2000,
            };
            Thread.Sleep(1000);

            while (repeat-- > 0 && !cts.IsCancellationRequested)
            {
                if (cts.IsCancellationRequested) break;
                try
                {
                    if (!modemSerialPort.IsOpen) modemSerialPort.Open();
                    modemSerialPort.WriteLine("at+cusbadb=1,1");
                }
                catch (Exception) { }
                finally { modemSerialPort.Close(); }
                Thread.Sleep(4000);
            }
        }, cts.Token);

        var loc = new ModemLocator(LocatorQuery.creationSimComADB, LocatorQuery.androidDevice);
        try                                                                                         //bad practice!!!
        {
            if (loc.GetDevices().Count() > 0)
            {
                cts.Cancel();
                task.Wait();
                return true;
            }
            loc.WaitDeviceAttach(new TimeSpan(0, 0, 28));
            cts.Cancel();
            task.Wait();
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    private void UpdateCfgSN()
    {
        var dir = Path.GetDirectoryName(WorkingDir);
        var factory = new FactoryCFG(dir);
        factory.UpdateFactory();
        DeviceSerial = factory.SerialNumber.ToString();
    }

    private Sensors LockCU()
    {
        _cu.SetOuts(Outs.Pn1 | Outs.Blue);
        StatusColor = Brushes.LightBlue;
        return _cu.WaitForState(Sensors.Lodg | Sensors.Device | Sensors.Pn1_Up, 2);
    }

    private bool EnsureCUState()
    {
        _cu.SetOuts(Outs.Pn1 | Outs.Blue);
        StatusColor = Brushes.LightBlue;
        return _cu.WaitForState(Sensors.Lodg | Sensors.Device | Sensors.Pn1_Up, 2) == (Sensors.Lodg | Sensors.Device | Sensors.Pn1_Up);
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

        var batchFile = Path.Combine(workingDir, adbBatch);
        if (!File.Exists(batchFile)) throw new FileNotFoundException("adb batch file not found");

        var batch = new Batch(batchFile, workingDir);

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

        using var serial = new SerialPort(portName)
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

    private bool AwaitDevice(string portName, string request, int timeout = 30)
    {
        var tcs = new TaskCompletionSource<bool>();
        var elapsed = 0;
        using var serial = new SerialPort(portName)
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
            serial.WriteLine(request);
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

    private bool AwaitDeviceStart(string portName, int timeout = Timeout.Infinite)
    {
        Thread.Sleep(timeout * 1000);

        using var serial = new SerialPort(portName)
        {
            Handshake = localHandshake,
            NewLine = localNewLine,
        };

        return ATWriter.SendCommand(serial, new ATCommand("AT+CGMM"));
    }

    private bool Old_AwaitDeviceStart(string portName, int timeout = Timeout.Infinite)
    {
        var tcs = new TaskCompletionSource<bool>();
        var command = "at";
        var elapsed = 0;
        using var serial = new SerialPort(portName)
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

    private string AwaitDeviceAttach(int timeout = 20)
    {
        var modemLocator = new ModemLocator(LocatorQuery.queryEventTelit, LocatorQuery.queryTelitModem);
        modemLocator.WaitDeviceAttach(TimeSpan.FromSeconds(timeout));
        return modemLocator.GetModemPortNames().First();
    }

    private void WriterFaultState()
    {
        ProgressIndeterminate = true;
        _cu.SetOuts(Outs.Red);
        StatusColor = Brushes.LightPink;
        FailValue++;
        LogMsg("Снятие питания...");
        var opResult = TurnModemPowerOff();
        if (opResult is not true)
            LogMsg("Failed to power off board");
        _cu.WaitForState(Sensors.Pn1_Down);
        ProgressIndeterminate = false;
    }

    private void WriterStopState()
    {
        //ProgressValue = 0;
        ProgressIndeterminate = true;
        _cu.SetOuts(Outs.None);
        StatusColor = Brushes.Wheat;
        LogMsg("Снятие питания...");
        var opResult = TurnModemPowerOff();
        if (opResult is not true)
            LogMsg("Failed to power off board");
        _cu.WaitForState(Sensors.Pn1_Down);
        ProgressIndeterminate = false;
    }

    private void WriterSuccessState(TimeSpan elapsed)
    {
        ProgressValue = 100;
        _cu.SetOuts(Outs.Green);
        StatusColor = Brushes.LightGreen;
        PassValue++;
        TimeAvgValue = elapsed;
        _cu.WaitForState(Sensors.Pn1_Down);
        ProgressValue = 0;
    }

    private bool TurnModemPowerOn() => _cu.PowerOn();

    private bool TurnModemPowerOff(int timeout = 2)
    {
        Thread.Sleep(TimeSpan.FromSeconds(timeout));

        _cu.PowerOff();

        return true;
    }

    private Sensors AwaitCUClose() => _cu.WaitForState(Sensors.Lodg | Sensors.Device | Sensors.Pn1_Down);

    public void LogMsg(string? message) => _kuLogList.Insert(0, message ?? string.Empty);
}

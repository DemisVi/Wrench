using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Wrench.View;
using Wrench.Model;
using Wrench.Services;
using System.Collections.ObjectModel;
using System.Windows.Data;
using System.IO;
using System.Threading;
using System.Windows.Interop;
using Wrench.Interfaces;

namespace Wrench.ViewModels;

public class MainViewModel : INotifyPropertyChanged
{
    private const string _dataDirName = "Data";
    private string? _dataDir;
    private IWriter? WriterCU1;
    private object _synclock1 = new();
    private readonly Validator _validator = new();
    private readonly List<string> _writerVariant = new() { "SimCom упр.", "SimCom ретро.", "Telit упр.", "Telit ретро." };

    public ObservableCollection<string> CU1LogList { get; set; } = new();
    public event PropertyChangedEventHandler? PropertyChanged;

    public MainViewModel()
    {
        //var currentDir = Directory.GetCurrentDirectory();
        //_dataDir = Path.Combine(currentDir, _dataDirName);

        //var dirs = new DirectoryInfo(_dataDir).EnumerateDirectories();
        //foreach (var i in dirs)
        //{
        //    DeviceType.Add(i.Name, new List<string>());
        //    foreach (var j in i.GetDirectories())
        //        DeviceType[i.Name].Add(j.Name);
        //}

        //WriterCU1 = new Writer(CU1LogList);
        //WriterCU1.PropertyChanged += WriterKU1_PropertyChanged;

        try
        {
            ContactUnitTitle = new AdapterLocator().AdapterSerials.First().Trim(new[] { 'A', 'B' });
        }
        catch (Exception)
        {
            StatusColor = Brushes.Red;
            ContactUnitTitle = "no adaper";
        }

        BindingOperations.EnableCollectionSynchronization(CU1LogList, _synclock1);
    }

    protected bool SetProperty<T>(ref T field, T newValue, [CallerMemberName] string? propertyName = null)
    {
        if (!Equals(field, newValue))
        {
            field = newValue;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            return true;
        }
        return false;
    }

    protected void OnPropertyChange([CallerMemberName] string? property = "") =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));

    private void WriterKU1_PropertyChanged(object? sender, PropertyChangedEventArgs e) =>  ///experimental crap
             GetType().GetProperty(e.PropertyName!)?.SetValue(this, (sender as IWriter)!
            .GetType()?.GetProperty(e.PropertyName!)?.GetValue(sender));

    private Command? _exit;
    public ICommand Exit => _exit ??= new Command(PerformExit);
    private void PerformExit(object? commandParameter) => (commandParameter as Window)?.Close();

    private Command? _showPackageSelector;
    public ICommand ShowPackageSelector => _showPackageSelector ??= new Command(PerformShowPackageSelector, x => !IsWriterRunning);
    private void PerformShowPackageSelector(object? commandParameter)
    {
        if (DeviceType is not null)
        {
            DeviceType?.Clear();
            OnPropertyChange(nameof(DeviceType));
        }
        else
            DeviceType = new();

        var currentDir = Directory.GetCurrentDirectory();
        var dataDir = SelectedWriter switch
        {
            "SimCom упр." => "SimCom_simple",
            "SimCom ретро." => "SimCom_retro",
            "Telit упр." => "Telit_simple",
            "Telit ретро." => "Telit_retro",
            _ => throw new NotImplementedException(),
        };

        _dataDir = Path.Combine(currentDir, dataDir);

        var dirs = new DirectoryInfo(_dataDir).EnumerateDirectories();
        foreach (var i in dirs)
        {
            DeviceType?.Add(i.Name, new List<string>());
            foreach (var j in i.GetDirectories())
                DeviceType?[i.Name].Add(j.Name);
        }
        OnPropertyChange(nameof(DeviceType));

        new PackageSelectorWindow(this).ShowDialog();
    }

    private Command? _startWriter;
    public ICommand StartWriter => _startWriter ??= new Command(PerformStartWriter, x => IsWriterRunningInvert && PackageDir.Length > 0);

    private void PerformStartWriter(object? commandParameter)
    {
        WriterCU1 = commandParameter switch
        {
            "SimCom упр." => new SimComWriter(CU1LogList, isOoo: IsOoo),
            "SimCom ретро." => new SimComWriter(CU1LogList, true, isOoo: IsOoo),
            "Telit упр." => throw new NotImplementedException(),
            "Telit ретро." => new TelitWriter(CU1LogList),
            _ => throw new NotImplementedException(),
        };

        WriterCU1.PropertyChanged += WriterKU1_PropertyChanged;
        WriterCU1.WorkingDir = PackageDir;
        IsWriterRunning = true;
        WriterCU1?.Start();
        FlashButtonColor = Brushes.IndianRed;
    }

    private Command? _stopWriter;
    public ICommand StopWriter => _stopWriter ??= new Command(PerformStopWriter, x => IsWriterRunning);
    private void PerformStopWriter(object? commandParameter)
    {
        if (WriterCU1 is not null)
        {
            WriterCU1.PropertyChanged -= WriterKU1_PropertyChanged;
            IsWriterRunning = false;
            WriterCU1.Stop();
        }
        FlashButtonColor = Brushes.Beige;
    }

    private Command? loadSelected;
    public ICommand? LoadSelected => loadSelected ??= new Command(PerformLoadSelected,
        x => !string.IsNullOrEmpty(SelectedVersion) && !string.IsNullOrEmpty(SelectedDevice));

    private void PerformLoadSelected(object? commandParameter)
    {
        if (string.IsNullOrEmpty(_dataDir)) throw new NullReferenceException(nameof(_dataDir) + " cannot be null");

        PackageDir = Path.Combine(_dataDir, SelectedDevice ?? string.Empty, SelectedVersion ?? string.Empty);
        //WriterCU1.WorkingDir = PackageDir;
        var msg = new StringBuilder().AppendJoin(' ', new[] { "Загружен пакет", SelectedDevice });
        if (SelectedVersion?.Length > 0) msg.AppendJoin(' ', new[] { " /", "версия", SelectedVersion });
        CU1LogList.Insert(0, msg.ToString() /*+ Environment.NewLine + PackageDir*/);
        (commandParameter as Window)?.Close();
        var dir = Path.GetDirectoryName(PackageDir);
        var factory = new FactoryCFG(dir);
        try
        {
            factory.ReadFactory();
            DeviceSerial = factory.SerialNumber.ToString();
        }
        catch (Exception)
        {
            DeviceSerial = "no serial";
        }
    }

    public List<string> WriterVariant => _writerVariant;

    private string? _contactUnit = null;
    public string? ContactUnitTitle { get => _contactUnit; set => SetProperty(ref _contactUnit, value, nameof(ContactUnitTitle)); }

    private string _passwordText = string.Empty;
    public string PasswordText
    {
        get => _passwordText; set
        {
            SetProperty(ref _passwordText, value);
            IsAccessGranted = _validator.IsValidationPassed(PasswordText);
            if (IsAccessGranted) IndicatorColor = Brushes.GreenYellow;
            else IndicatorColor = Brushes.Beige;
        }
    }

    private bool _isWriterRunning = false;
    public bool IsWriterRunning
    {
        get => _isWriterRunning; set
        {
            SetProperty(ref _isWriterRunning, value);
            OnPropertyChange(nameof(IsWriterRunningInvert));
        }
    }
    public bool IsWriterRunningInvert => !_isWriterRunning;

    private string _selectedDevice = string.Empty;
    public string SelectedDevice
    {
        get => _selectedDevice; set
        {
            SetProperty(ref _selectedDevice, value);
            if (_selectedDevice is not null)
                DeviceVersion = DeviceType[_selectedDevice];
        }
    }

    private bool _enableWriterToggle = true;
    public bool EnableWriterToggle { get => _enableWriterToggle; set => SetProperty(ref _enableWriterToggle, value); }

    private string _packageDir = string.Empty;
    public string PackageDir { get => _packageDir; set => SetProperty(ref _packageDir, value); }

    private string _selectedVersion = string.Empty;
    public string SelectedVersion { get => _selectedVersion; set => SetProperty(ref _selectedVersion, value); }

    private string _operationStatus = string.Empty;
    public string OperationStatus { get => _operationStatus; set => SetProperty(ref _operationStatus, value); }

    private string _deviceSerial = string.Empty;
    public string DeviceSerial
    {
        get
        {
            try
            {
                return string.Format("{0} ({1})", _deviceSerial, _deviceSerial.ToInt32());
            }
            catch (Exception)
            {
                return "no serial";
            }
        }
        set => SetProperty(ref _deviceSerial, value);
    }


    private string _selectedWriter = string.Empty;
    public string SelectedWriter { get => _selectedWriter; set => SetProperty(ref _selectedWriter, value); }

    private bool _isAccessGranted = false;
    public bool IsAccessGranted { get => _isAccessGranted; set => SetProperty(ref _isAccessGranted, value); }

    private int _progressValue = 0;
    public int ProgressValue { get => _progressValue; set => SetProperty(ref _progressValue, value); }

    private TimeSpan _operationTime = TimeSpan.Zero;
    public TimeSpan OperationTime { get => _operationTime; set => SetProperty(ref _operationTime, value); }

    private bool _progressIndeterminate = false;
    public bool ProgressIndeterminate { get => _progressIndeterminate; set => SetProperty(ref _progressIndeterminate, value); }

    private bool _onTop = false;
    public bool OnTop { get => _onTop; set => SetProperty(ref _onTop, value); }

    private Brush _indicatorColor = Brushes.Beige;
    public Brush IndicatorColor { get => _indicatorColor; set => SetProperty(ref _indicatorColor, value); }

    private Brush _flashButtonColor = Brushes.Beige;
    public Brush FlashButtonColor { get => _flashButtonColor; set => SetProperty(ref _flashButtonColor, value); }

    private Dictionary<string, List<string>> _deviceType = new Dictionary<string, List<string>>();
    public Dictionary<string, List<string>> DeviceType { get => _deviceType; set => SetProperty(ref _deviceType, value); }

    private List<string> _deviceVersion = new List<string>();
    public List<string> DeviceVersion { get => _deviceVersion; set => SetProperty(ref _deviceVersion, value); }

    private Brush _statusColor = Brushes.White;
    public Brush StatusColor { get => _statusColor; set => SetProperty(ref _statusColor, value); }

    private int _passValue;
    public int PassValue
    {
        get => _passValue; set
        {

            SetProperty(ref _passValue, value);
            OnPropertyChange(nameof(PFKValue));
        }
    }

    private int _failValue;
    public int FailValue
    {
        get => _failValue; set
        {
            SetProperty(ref _failValue, value);
            OnPropertyChange(nameof(PFKValue));
        }
    }

    public string PFKValue { get => string.Format($"{((float)PassValue / (float)(PassValue + FailValue)):N2}"); }

    private TimeSpan _timeAvgValue;
    public TimeSpan TimeAvgValue
    {
        get
        {
            if (PassValue <= 0) return TimeSpan.Zero;

            return new TimeSpan(0, 0, (int)(_timeAvgValue / PassValue).TotalSeconds);
        }
        set => SetProperty(ref _timeAvgValue, _timeAvgValue + value);
    }

    private bool _isOoo = true;
    public bool IsOoo { get => _isOoo; set => SetProperty(ref _isOoo, value); }
}

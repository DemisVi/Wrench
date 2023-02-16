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

namespace Wrench.ViewModels;

public class MainViewModel : INotifyPropertyChanged
{
    private const string _dataDirName = "Data";
    private readonly string _dataDir;
    private Writer WriterCU1;
    private object _synclock1 = new();
    private readonly Validator _validator = new();

    public ObservableCollection<string> CU1LogList { get; set; } = new();
    public event PropertyChangedEventHandler? PropertyChanged;

    public MainViewModel()
    {
        var currentDir = Directory.GetCurrentDirectory();
        _dataDir = Path.Combine(currentDir, _dataDirName);

        var dirs = new DirectoryInfo(_dataDir).EnumerateDirectories();
        foreach (var i in dirs)
        {
            DeviceType.Add(i.Name, new List<string>());
            foreach (var j in i.GetDirectories())
                DeviceType[i.Name].Add(j.Name);
        }

        WriterCU1 = new Writer(CU1LogList);
        WriterCU1.PropertyChanged += WriterKU1_PropertyChanged;

        ContactUnit = new AdapterLocator().AdapterSerials.First().Trim(new[] { 'A', 'B' });

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
             GetType().GetProperty(e.PropertyName!)?.SetValue(this, (sender as Writer)!
            .GetType()?.GetProperty(e.PropertyName!)?.GetValue(sender));

    private Command? _exit;
    public ICommand Exit => _exit ??= new Command(PerformExit);
    private void PerformExit(object? commandParameter) => (commandParameter as Window)?.Close();

    private Command? _showPackageSelector;
    public ICommand ShowPackageSelector => _showPackageSelector ??= new Command(PerformShowPackageSelector, x => !IsWriterRunning);
    private void PerformShowPackageSelector(object? commandParameter)
    {
        new PackageSelectorWindow(this).Show();
    }

    private Command? _toggleWriter;
    public ICommand ToggleWriter => _toggleWriter ??= new Command(PerformToggleWriter, x => PackageDir.Length > 0);

    private void PerformToggleWriter(object? commandParameter)
    {
        if (!IsWriterRunning)
        {
            WriterCU1?.Start();
            FlashButtonColor = Brushes.IndianRed;

        }
        else
        {
            WriterCU1.Stop();
            FlashButtonColor = Brushes.Beige;
        }
        IsWriterRunning = !IsWriterRunning;
    }

    private Command? loadSelected;
    public ICommand? LoadSelected => loadSelected ??= new Command(PerformLoadSelected, x => !string.IsNullOrEmpty(SelectedVersion));

    private void PerformLoadSelected(object? commandParameter)
    {
        PackageDir = Path.Combine(_dataDir, SelectedDevice ?? string.Empty, SelectedVersion ?? string.Empty);
        WriterCU1.WorkingDir = PackageDir;
        var msg = new StringBuilder().AppendJoin(' ', new[] { "Загружен пакет", SelectedDevice });
        if (SelectedVersion?.Length > 0) msg.AppendJoin(' ', new[] { " /", "версия", SelectedVersion });
        CU1LogList.Insert(0, msg.ToString() /*+ Environment.NewLine + PackageDir*/);
        (commandParameter as Window)?.Close();
    }

    private string? _contactUnit = null;
    public string? ContactUnit { get => _contactUnit; set => SetProperty(ref _contactUnit, value, nameof(ContactUnit)); }

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
    public bool? IsWriterRunningInvert => !_isWriterRunning;

    private string _selectedDevice = string.Empty;
    public string SelectedDevice
    {
        get => _selectedDevice; set
        {
            SetProperty(ref _selectedDevice, value);
            DeviceVersion = DeviceType[SelectedDevice];
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
    public string DeviceSerial { get => _deviceSerial; set => SetProperty(ref _deviceSerial, value); }

    private bool _isAccessGranted = false;
    public bool IsAccessGranted { get => _isAccessGranted; set => SetProperty(ref _isAccessGranted, value); }

    private int _progressValue = 0;
    public int ProgressValue { get => _progressValue; set => SetProperty(ref _progressValue, value); }

    private bool _progressIndeterminate = false;
    public bool ProgressIndeterminate { get => _progressIndeterminate; set => SetProperty(ref _progressIndeterminate, value); }

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

    public string PFKValue { get => string.Format("{0}/{1}", PassValue, FailValue); }

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
}

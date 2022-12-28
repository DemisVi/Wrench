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
    private Writer WriterKU1;
    private object _synclock1 = new();
    private readonly Validator _validator = new();

    public ObservableCollection<string> KU1LogList { get; set; } = new();
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

        WriterKU1 = new Writer(KU1LogList);
        WriterKU1.PropertyChanged += WriterKU1_PropertyChanged;

        BindingOperations.EnableCollectionSynchronization(KU1LogList, _synclock1);
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
    public ICommand ShowPackageSelector => _showPackageSelector ??= new Command(PerformShowPackageSelector, x => !(_isWriterRunning ?? false));
    private void PerformShowPackageSelector(object? commandParameter)
    {
        new PackageSelectorWindow(this).Show();
    }

    private Command? _toggleWriter;
    public ICommand ToggleWriter => _toggleWriter ??= new Command(PerformToggleWriter, x => (EnableWriterToggle ?? false));
    private void PerformToggleWriter(object? commandParameter) => WriterKU1?.Run();

    private Command? loadSelected;
    public ICommand? LoadSelected => loadSelected ??= new Command(PerformLoadSelected);
    private void PerformLoadSelected(object? commandParameter)
    {
        PackageDir = Path.Combine(_dataDir, SelectedDevice ?? string.Empty, SelectedVersion ?? string.Empty);
        var msg = new StringBuilder().AppendJoin(' ', new[] { "Загружен пакет", SelectedDevice });
        if (SelectedVersion?.Length > 0) msg.AppendJoin(' ', new[] { " /", "версия", SelectedVersion });
        KU1LogList.Add(msg.ToString());
        (commandParameter as Window)?.Close();
    }

    private string? _passwordText;
    public string? PasswordText
    {
        get => _passwordText; set
        {
            SetProperty(ref _passwordText, value);
            IsAccessGranted = _validator.IsValidationPassed(PasswordText);
            if (IsAccessGranted) IndicatorColor = Brushes.GreenYellow;
            else IndicatorColor = Brushes.Beige;
        }
    }

    private bool? _isWriterRunning = false;
    public bool? IsWriterRunning
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

    private bool? _enableWriterToggle = true;
    public bool? EnableWriterToggle { get => _enableWriterToggle; set => SetProperty(ref _enableWriterToggle, value); }

    private string? _packageDir;
    public string? PackageDir { get => _packageDir; set => SetProperty(ref _packageDir, value); }

    private string _selectedVersion = string.Empty;
    public string SelectedVersion { get => _selectedVersion; set => SetProperty(ref _selectedVersion, value); }

    private string? _operationStatus = "";
    public string? OperationStatus { get => _operationStatus; set => SetProperty(ref _operationStatus, value); }

    private bool _isAccessGranted;
    public bool IsAccessGranted { get => _isAccessGranted; set => SetProperty(ref _isAccessGranted, value); }

    private Brush _indicatorColor = Brushes.Beige;
    public Brush IndicatorColor { get => _indicatorColor; set => SetProperty(ref _indicatorColor, value); }

    private Dictionary<string, List<string>> _deviceType = new Dictionary<string, List<string>>();
    public Dictionary<string, List<string>> DeviceType { get => _deviceType; set => SetProperty(ref _deviceType, value); }

    private List<string> _deviceVersion = new List<string>();
    public List<string> DeviceVersion { get => _deviceVersion; set => SetProperty(ref _deviceVersion, value); }
}

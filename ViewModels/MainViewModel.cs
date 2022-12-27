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

namespace Wrench.ViewModels
{
    internal class MainViewModel : INotifyPropertyChanged
    {
        private const string _dataDir = "Data";
        private Writer WriterKU1;
        private object _synclock1 = new();
        private readonly Validator _validator = new();

        public ObservableCollection<string> KU1LogList { get; set; } = new();
        public event PropertyChangedEventHandler? PropertyChanged;

        public MainViewModel()
        {
            BindingOperations.EnableCollectionSynchronization(KU1LogList, _synclock1);
            WriterKU1 = new Writer(KU1LogList);
            DeviceType = new Dictionary<string, List<string>>();
            WriterKU1.PropertyChanged += WriterKU1_PropertyChanged;

            var dir = new DirectoryInfo(Path.Combine(Directory.GetCurrentDirectory(), _dataDir)).EnumerateDirectories();
            foreach (var i in dir)
                DeviceType.Add(i.Name, new List<string>());
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

        protected Command? _showSettings;
        public ICommand ShowSettings => _showSettings ??= new Command(PerformShowSettings, obj => IsAccessGranted);
        private void PerformShowSettings(object? obj) => new SettingsWindow().ShowDialog();

        private Command? _exit;
        public ICommand Exit => _exit ??= new Command(PerformExit);
        private void PerformExit(object? commandParameter) => Environment.Exit(0);

        private Command? _showPackageSelector;
        public ICommand ShowPackageSelector => _showPackageSelector ??= new Command(PerformShowPackageSelector, x => !(_isWriterRunning ?? false));
        private void PerformShowPackageSelector(object? commandParameter) => new PackageSelectorWindow().Show();

        private Command? stopWriters;
        public ICommand StopWriters => stopWriters ??= new Command(PerformStopWriters);
        private void PerformStopWriters(object? commandParameter) => WriterKU1.Run();

        private Brush _indicatorColor = Brushes.Beige;
        public Brush IndicatorColor { get => _indicatorColor; set => SetProperty(ref _indicatorColor, value); }

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

        private string? _operationStatus = "";
        public string? OperationStatus { get => _operationStatus; set => SetProperty(ref _operationStatus, value); }

        private bool _isAccessGranted;
        public bool IsAccessGranted { get => _isAccessGranted; set => SetProperty(ref _isAccessGranted, value); }

        private Dictionary<string, List<string>>? deviceType;
        public Dictionary<string, List<string>>? DeviceType { get => deviceType; set => SetProperty(ref deviceType, value); }

        private List<string>? deviceVersion;
        public List<string>? DeviceVersion { get => deviceVersion; set => SetProperty(ref deviceVersion, value); }
    }
}

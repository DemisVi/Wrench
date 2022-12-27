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

namespace Wrench.ViewModels
{
    internal class MainViewModel : INotifyPropertyChanged
    {
        private Writer WriterKU1;
        private object _synclock1 = new();
        private readonly Validator _validator = new();

        public ObservableCollection<string> KU1LogList { get; set; } = new();
        public event PropertyChangedEventHandler? PropertyChanged;

        public MainViewModel()
        {
            WriterKU1 = new Writer(KU1LogList);
            BindingOperations.EnableCollectionSynchronization(KU1LogList, _synclock1);

            WriterKU1.PropertyChanged += WriterKU1_PropertyChanged;

            DeviceType = new List<string>() {"18.3879600 - 54 АВЭОС", "18.3879600-54 АВЭОС 24В",
                                             "18.3879600-70 ГАЗ",
                                             "18.3879600-75 УАЗ",
                                             "1824.3879600-42_БЭГ_ЛИАЗ 24В",
                                             "1824_3879600-40 ПАЗ",
                                             "1824_3879600-41 КАВЗ 24В",
                                             "8450092997 Ларгус",
                                             "8450110539 Гранта" };
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

        private bool? _isWriterRunning;
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

        private List<string>? deviceType;

        public List<string>? DeviceType { get => deviceType; set => SetProperty(ref deviceType, value); }

        private List<string>? deviceVersion;

        public List<string>? DeviceVersion { get => deviceVersion; set => SetProperty(ref deviceVersion, value); }
    }
}

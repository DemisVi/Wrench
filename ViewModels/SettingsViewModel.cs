using System;
using System.Management;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Wrench.Services;
using Wrench.View;
using System.Windows;
using Wrench.Properties;
using Wrench.ViewModels;

namespace Wrench.ViewModels
{
    internal class SettingsViewModel : INotifyPropertyChanged
    {
        private AdapterLocator _adapterLocator = new();
        private ModemLocator _modemLocator = new(LocatorQuery.queryEventTelit, LocatorQuery.queryTelitModem);
        private Adapter _adapterKU = new();

        public event PropertyChangedEventHandler? PropertyChanged;

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

        protected void OnPropertyChange([CallerMemberName] string? property = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }

        private ICommand? _close;
        public ICommand Close { get { return _close ??= new Command(PerformClose); } }
        private void PerformClose(object? commandParameter) => (commandParameter as SettingsWindow)?.Close();

        private ICommand? _turnOnKL30;
        public ICommand TurnOnKL30 => _turnOnKL30 ??= new Command(PerformTurnOnKL30);
        private void PerformTurnOnKL30(object? obj)
        {
            if (obj == null) return;
            if (!_adapterKU.IsOpen)
            {
                _adapterKU.OpenAdapter((string)obj);
                _adapterKU.KL30_On();
                OnPropertyChange(nameof(KU1AdapterOpened));
            }
            else
            {
                _adapterKU.KL30_Off();
                _adapterKU.CloseAdapter();
                OnPropertyChange(nameof(KU1AdapterOpened));
            }
        }

        private ICommand? _reloadaPorts;
        public ICommand ReloadPorts => _reloadaPorts ??= new Command(PerformReloadPorts);
        private void PerformReloadPorts(object? obj)
        {
            ModemPortNames = _modemLocator.GetModemPortNames();
        }

        private ICommand? _saveSettings;
        public ICommand SaveSettings => _saveSettings ??= new Command(PerformSaveSettings);
        private void PerformSaveSettings(object? commandParameter) => Settings.Default.Save();

        public string KU1AdapterSerial
        {
            get => Settings.Default.AdapterKU1;
            set => Settings.Default.AdapterKU1 = value;
        }

        public bool KU1AdapterOpened => _adapterKU.IsOpen;
        public List<string> AdapterSerials { get => _adapterLocator.AdapterSerials; }

        private List<string> _modemPortNames = new();
        public List<string> ModemPortNames 
        { 
            get => _modemLocator.GetModemPortNames();
            set => SetProperty(ref _modemPortNames, value, nameof(ModemPortNames));
        }
    }
}

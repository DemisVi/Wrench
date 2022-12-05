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
using Wrench.Model;

namespace Wrench.ViewModels
{
    internal class SettingsViewModel : INotifyPropertyChanged
    {
        private AdapterLocator _adapterLocator = new();
        private ModemLocator _modemLocator = new(LocatorQuery.queryEventTelit, LocatorQuery.queryTelitModem);
        private Adapter _adapter1 = new(Settings.Default.AdapterKU1);
        private Adapter _adapter2 = new(Settings.Default.AdapterKU2);
        private Adapter _adapter3 = new(Settings.Default.AdapterKU3);
        private Adapter _adapter4 = new(Settings.Default.AdapterKU4);
        private Adapter _adapter5 = new(Settings.Default.AdapterKU5);
        private Adapter _adapter6 = new(Settings.Default.AdapterKU6);

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

        private ICommand? _turnOnKL301;
        public ICommand TurnOnKL301 => _turnOnKL301 ??= new Command(PerformTurnOnKL30Adapter2);
        private void PerformTurnOnKL30Adapter2(object? obj)
        {
            if (obj == null) return;
            if (!_adapter1.IsOpen)
            {
                _adapter1.OpenAdapter();
                _adapter1.KL30_On();
                KU1AdapterOpened = _adapter1.IsOpen;
            }
            else
            {
                _adapter1.KL30_Off();
                _adapter1.CloseAdapter();
                KU1AdapterOpened = _adapter1.IsOpen;
            }
        }

        private ICommand? _turnOnKL302;
        public ICommand TurnOnKL302 => _turnOnKL302 ??= new Command(PerformTurnOnKL30Adapter3);
        private void PerformTurnOnKL30Adapter3(object? obj)
        {
            if (obj == null) return;
            if (!_adapter2.IsOpen)
            {
                _adapter2.OpenAdapter();
                _adapter2.KL30_On();
                KU2AdapterOpened = _adapter2.IsOpen;
            }
            else
            {
                _adapter2.KL30_Off();
                _adapter2.CloseAdapter();
                KU2AdapterOpened = _adapter2.IsOpen;
            }
        }

        private ICommand? _turnOnKL303;
        public ICommand TurnOnKL303 => _turnOnKL303 ??= new Command(PerformTurnOnKL30Adapter4);
        private void PerformTurnOnKL30Adapter4(object? obj)
        {
            if (obj == null) return;
            if (!_adapter3.IsOpen)
            {
                _adapter3.OpenAdapter();
                _adapter3.KL30_On();
                KU3AdapterOpened = _adapter3.IsOpen;
            }
            else
            {
                _adapter3.KL30_Off();
                _adapter3.CloseAdapter();
                KU3AdapterOpened = _adapter3.IsOpen;
            }
        }

        private ICommand? _turnOnKL304;
        public ICommand TurnOnKL304 => _turnOnKL304 ??= new Command(PerformTurnOnKL30Adapter5);
        private void PerformTurnOnKL30Adapter5(object? obj)
        {
            if (obj == null) return;
            if (!_adapter4.IsOpen)
            {
                _adapter4.OpenAdapter();
                _adapter4.KL30_On();
                KU4AdapterOpened = _adapter4.IsOpen;
            }
            else
            {
                _adapter4.KL30_Off();
                _adapter4.CloseAdapter();
                KU4AdapterOpened = _adapter4.IsOpen;
            }
        }

        private ICommand? _turnOnKL305;
        public ICommand TurnOnKL305 => _turnOnKL305 ??= new Command(PerformTurnOnKL30Adapter6);
        private void PerformTurnOnKL30Adapter6(object? obj)
        {
            if (obj == null) return;
            if (!_adapter5.IsOpen)
            {
                _adapter5.OpenAdapter();
                _adapter5.KL30_On();
                KU5AdapterOpened = _adapter5.IsOpen;
            }
            else
            {
                _adapter5.KL30_Off();
                _adapter5.CloseAdapter();
                KU5AdapterOpened = _adapter5.IsOpen;
            }
        }

        private ICommand? _turnOnKL306;
        public ICommand TurnOnKL306 => _turnOnKL306 ??= new Command(PerformTurnOnKL30Adapter1);
        private void PerformTurnOnKL30Adapter1(object? obj)
        {
            if (obj == null) return;
            if (!_adapter6.IsOpen)
            {
                _adapter6.OpenAdapter();
                _adapter6.KL30_On();
                KU6AdapterOpened = _adapter6.IsOpen;
            }
            else
            {
                _adapter6.KL30_Off();
                _adapter6.CloseAdapter();
                KU6AdapterOpened = _adapter6.IsOpen;
            }
        }

        private ICommand? _reloadaPorts;
        public ICommand ReloadPorts => _reloadaPorts ??= new Command(PerformReloadPorts);
        private void PerformReloadPorts(object? obj)
        {
            _adapterLocator.Rescan();
            OnPropertyChange(nameof(ModemPortNames));
            OnPropertyChange(nameof(AdapterSerials));
        }

        private ICommand? _saveSettings;
        public ICommand SaveSettings => _saveSettings ??= new Command(PerformSaveSettings);
        private void PerformSaveSettings(object? commandParameter)
        {
            _adapter1 = new(Settings.Default.AdapterKU1);
            _adapter2 = new(Settings.Default.AdapterKU2);
            _adapter3 = new(Settings.Default.AdapterKU3);
            _adapter4 = new(Settings.Default.AdapterKU4);
            _adapter5 = new(Settings.Default.AdapterKU5);
            _adapter6 = new(Settings.Default.AdapterKU6);

            Settings.Default.Save();
        }

        public string KU1AdapterSerial
        {
            get => Settings.Default.AdapterKU1;
            set => Settings.Default.AdapterKU1 = value;
        }

        public string KU2AdapterSerial
        {
            get => Settings.Default.AdapterKU2;
            set => Settings.Default.AdapterKU2 = value;
        }

        public string KU3AdapterSerial
        {
            get => Settings.Default.AdapterKU3;
            set => Settings.Default.AdapterKU3 = value;
        }

        public string KU4AdapterSerial
        {
            get => Settings.Default.AdapterKU4;
            set => Settings.Default.AdapterKU4 = value;
        }

        public string KU5AdapterSerial
        {
            get => Settings.Default.AdapterKU5;
            set => Settings.Default.AdapterKU5 = value;
        }

        public string KU6AdapterSerial
        {
            get => Settings.Default.AdapterKU6;
            set => Settings.Default.AdapterKU6 = value;
        }

        public string ModemSerialKU1
        {
            get => Settings.Default.SerialKU1;
            set => Settings.Default.SerialKU1 = value;
        }

        public string ModemSerialKU2
        {
            get => Settings.Default.SerialKU2;
            set => Settings.Default.SerialKU2 = value;
        }

        public string ModemSerialKU3
        {
            get => Settings.Default.SerialKU3;
            set => Settings.Default.SerialKU3 = value;
        }

        public string ModemSerialKU4
        {
            get => Settings.Default.SerialKU4;
            set => Settings.Default.SerialKU4 = value;
        }

        public string ModemSerialKU5
        {
            get => Settings.Default.SerialKU5;
            set => Settings.Default.SerialKU5 = value;
        }

        public string ModemSerialKU6
        {
            get => Settings.Default.SerialKU6;
            set => Settings.Default.SerialKU6 = value;
        }

        private bool _ku1AdapterOpened = false;
        public bool KU1AdapterOpened { get => _ku1AdapterOpened; set => SetProperty(ref _ku1AdapterOpened, value); }

        private bool _ku2AdapterOpened = false;
        public bool KU2AdapterOpened { get => _ku2AdapterOpened; set => SetProperty(ref _ku2AdapterOpened, value); }

        private bool _ku3AdapterOpened = false;
        public bool KU3AdapterOpened { get => _ku3AdapterOpened; set => SetProperty(ref _ku3AdapterOpened, value); }

        private bool _ku4AdapterOpened = false;
        public bool KU4AdapterOpened { get => _ku4AdapterOpened; set => SetProperty(ref _ku4AdapterOpened, value); }

        private bool _ku5AdapterOpened = false;
        public bool KU5AdapterOpened { get => _ku5AdapterOpened; set => SetProperty(ref _ku5AdapterOpened, value); }

        private bool _ku6AdapterOpened = false;
        public bool KU6AdapterOpened { get => _ku6AdapterOpened; set => SetProperty(ref _ku6AdapterOpened, value); }

        public List<string> AdapterSerials { get => _adapterLocator.AdapterSerials; }
        public List<string> ModemPortNames { get => _modemLocator.GetModemPortNames(); }
    }
}

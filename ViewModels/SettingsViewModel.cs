using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Wrench.Services;
using Wrench.View;

namespace Wrench.ViewModels
{
    internal class SettingsViewModel : INotifyPropertyChanged
    {
        private AdapterLocator _adapterLocator = new();

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

        private ICommand? _close;
        public ICommand Close { get { return _close ??= new Command(PerformClose); } }
        private void PerformClose(object? commandParameter) => (commandParameter as SettingsWindow)?.Close();

        public List<string> AdapterSerials { get => _adapterLocator.AdapterSerials; }
    }
}

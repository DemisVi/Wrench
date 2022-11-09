using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Wrench.ViewModels;

namespace Wrench.ViewModels
{
    internal class WorkerViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string property = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }

        protected bool SetProperty<T>(ref T field, T newValue, [CallerMemberName] string propertyName = null)
        {
            if (!Equals(field, newValue))
            {
                field = newValue;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
                return true;
            }

            return false;
        }

        private int _counter;

        public int Counter
        {
            get { return _counter; }
            set
            {
                _counter = value;
                OnPropertyChanged(nameof(Counter));
            }
        }

        public ICommand DoCommand
        {
            get
            {
                return new Command((obj) => Counter += 10,
                                   (obj) => Counter < 100);
            }
        }

        public ICommand UndoCommand
        {
            get
            {
                return new Command((obj) => Counter -= 10,
                                   (obj) => Counter > 0);
            }
        }

        private Command? exit;
        public ICommand Exit => exit ??= new Command(PerformExit);

        private void PerformExit(object commandParameter)
        {
            Environment.Exit(0);
        }

        private string _operationStatus = "Operation status";

        public string OperationStatus { get => _operationStatus; set => SetProperty(ref _operationStatus, value); }
    }
}

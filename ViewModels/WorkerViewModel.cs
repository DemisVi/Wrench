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

        private int _counter;

        public int Counter
        {
            get { return _counter; }
            set { SetProperty(ref _counter, value); }
        }

        private ICommand? _doCommand;
        public ICommand DoCommand => _doCommand ??= new Command(PerformDoCommand, (obj) => Counter < 100);

        private void PerformDoCommand(object? commandParameter)
        {
            Counter += 10;
            OperationStatus += "+";
        }

        private ICommand? _undoCommand;
        public ICommand UndoCommand => _undoCommand ??= new Command(PerformUndoCommand, (obj) => Counter > 0);
        private void PerformUndoCommand(object? commandParameter)
        {
            Counter -= 10;
            OperationStatus = OperationStatus.Remove(OperationStatus.LastIndexOf("+"));
        }

        private ICommand? exit;
        public ICommand Exit => exit ??= new Command(PerformExit);

        private void PerformExit(object? commandParameter)
        {
            Environment.Exit(0);
        }

        private string _operationStatus = "Operation status";

        public string OperationStatus { get => _operationStatus; set => SetProperty(ref _operationStatus, value); }
    }
}

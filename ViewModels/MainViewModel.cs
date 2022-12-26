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
        BackgroundWorker wrkr = new();
        public MainViewModel()
        {
            wrkr.DoWork += Wrkr_DoWork;
            WriterKU1 = new Writer(KU1LogList);
            BindingOperations.EnableCollectionSynchronization(KU1LogList, _synclock1);

            WriterKU1.PropertyChanged += WriterKU1_PropertyChanged;
        }

        private void WriterKU1_PropertyChanged(object? sender, PropertyChangedEventArgs e) =>  ///experimental crap
                this.GetType().GetProperty(e.PropertyName!)?.SetValue(this, (sender as Writer)!
                .GetType()?.GetProperty(e.PropertyName!)?.GetValue(sender));

        private void Wrkr_DoWork(object? sender, DoWorkEventArgs e)
        {
            KU1LogList.Add("wrkr doung wrk");
            WriterKU1.Prop();
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        private readonly Validator _validator = new();

        private object _synclock1 = new();

        private Writer WriterKU1;

        public ObservableCollection<string> KU1LogList { get; set; } = new();

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

        protected ICommand? _showSettings;
        public ICommand ShowSettings => _showSettings ??= new Command(PerformShowSettings, obj => IsAccessed);
        private void PerformShowSettings(object? obj) => new SettingsWindow().ShowDialog();

        private ICommand? _exit;
        public ICommand Exit => _exit ??= new Command(PerformExit);
        private void PerformExit(object? commandParameter) => Environment.Exit(0);

        private Command? startWriters;
        public ICommand StartWriters => startWriters ??= new Command(PerformStartWriters, x => OperationStatus?.Length < 40);

        private void PerformStartWriters(object? commandParameter)
        {
            WriterKU1.Set();
        }

        private Command? stopWriters;
        public ICommand StopWriters => stopWriters ??= new Command(PerformStopWriters, x => OperationStatus?.Length > 0);

        private void PerformStopWriters(object? commandParameter)
        {
            WriterKU1.Reset();
            WriterKU1.Prop();
        }

        private bool _isAccessed;
        public bool IsAccessed { get => _isAccessed; set => SetProperty(ref _isAccessed, value); }

        private string? _passwordText;
        public string? PasswordText
        {
            get => _passwordText; set
            {
                SetProperty(ref _passwordText, value);
                IsAccessed = _validator.IsValidationPassed(PasswordText);
                if (IsAccessed) IndicatorColor = Brushes.GreenYellow;
                else IndicatorColor = Brushes.Beige;
            }
        }

        private string? _operationStatus = "";
        public string? OperationStatus { get => _operationStatus; set => SetProperty(ref _operationStatus, value); }

        private System.Windows.Media.Brush _indicatorColor = Brushes.Beige;
        public System.Windows.Media.Brush IndicatorColor { get => _indicatorColor; set => SetProperty(ref _indicatorColor, value); }
    }
}

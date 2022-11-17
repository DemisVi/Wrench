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
using Wrench.Services;

namespace Wrench.ViewModels
{
    internal class MainViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        private readonly Validator _validator = new();

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
        public ICommand ShowSettings => _showSettings ??= new Command(PerformShowSettings);
        private void PerformShowSettings(object? obj)
        {
            new SettingsWindow().ShowDialog();
        }

        private ICommand? _validate;
        public ICommand Validate => _validate ??= new Command(PerformValidate);
        private void PerformValidate(object? commandParameter)
        {
            IsAccessed = _validator.IsValidationPassed(commandParameter as string);
            if (IsAccessed) IndicatorColor = Brushes.GreenYellow;
        }

        private ICommand? _invalidate;
        public ICommand Invalidate => _invalidate ??= new Command(PerformInvalidate);
        private void PerformInvalidate(object? commandParameter)
        {
            IsAccessed = false;
            PasswordText = null;
            IndicatorColor = Brushes.Beige;
        }

        private ICommand? _exit;
        public ICommand Exit => _exit ??= new Command(PerformExit);
        private void PerformExit(object? commandParameter)
        {
            Environment.Exit(0);
        }

        private int _counter;
        public int Counter { get => _counter; set => SetProperty(ref _counter, value); }

        private bool _isAccessed;
        public bool IsAccessed { get => _isAccessed; set => SetProperty(ref _isAccessed, value); }

        private string? _passwordText;
        public string? PasswordText { get => _passwordText; set => SetProperty(ref _passwordText, value); }

        private string _operationStatus = "Operation status";
        public string OperationStatus { get => _operationStatus; set => SetProperty(ref _operationStatus, value); }

        private System.Windows.Media.Brush _indicatorColor = Brushes.Beige;
        public System.Windows.Media.Brush IndicatorColor { get => _indicatorColor; set => SetProperty(ref _indicatorColor, value); }
    }
}

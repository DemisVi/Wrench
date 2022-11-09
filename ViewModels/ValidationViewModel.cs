using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Wrench.ViewModels
{
    internal class ValidationViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        public void OnPropertyChanged([CallerMemberName] string property = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }

        private string? _passwordText;
        public string? PasswordText
        {
            get { return _passwordText; }
            set
            {
                _passwordText = value;
                OnPropertyChanged(nameof(PasswordText));
            }
        }

        private Command? validate;
        public ICommand Validate => validate ??= new Command(PerformValidate);

        private void PerformValidate(object commandParameter)
        {
            MessageBox.Show((string)commandParameter, "Ololo!");
            PasswordText = string.Empty;
        }
    }
}

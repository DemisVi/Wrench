using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Wrench.ViewModels
{
    internal class WorkerViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        public void OnPropertyChanged([CallerMemberName] string property = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
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
    }
}

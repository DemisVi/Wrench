using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using ReactiveUI;
using Wrench.Views;

namespace Wrench.ViewModels;

public class LogViewModel : ViewModelBase
{
    public ObservableCollection<string> Log { get; set; } = new();

    public LogViewModel()
    {
    }
}

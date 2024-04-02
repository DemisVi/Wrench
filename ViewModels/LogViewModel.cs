using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Threading;
using ReactiveUI;
using Wrench.Views;

namespace Wrench.ViewModels;

public class LogViewModel : ViewModelBase
{
    private int flasherProgress;

    public ObservableCollection<string> Log { get; set; } = new();
    public int FlasherProgress { get => flasherProgress; set => this.RaiseAndSetIfChanged(ref flasherProgress, value); }
}

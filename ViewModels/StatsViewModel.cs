using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections.ObjectModel;
using System.Management;
using Wrench.Models;
using Wrench.Views;
using ReactiveUI;
using System.Reactive;

namespace Wrench.ViewModels;

public class StatsViewModel : ViewModelBase
{
    public ObservableCollection<Modem> Items { get; }
    public StatsViewModel(IEnumerable<Modem> items)
    {
        Items = new ObservableCollection<Modem>(items);
    }
}

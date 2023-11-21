using System;
using System.IO.Ports;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using ReactiveUI;
using Wrench.Models;

namespace Wrench.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    private ViewModelBase contentViewModel;
    
    public ViewModelBase ContentViewModel
    {
        get => contentViewModel;
        private set => this.RaiseAndSetIfChanged(ref contentViewModel, value);
    }

    //this has a dependency on the ToDoListService

    public MainWindowViewModel()
    {
        StatsVM = new(SerialPort.GetPortNames().Select(x => new Modem() { AttachedTo = x }));
        PackageSelectorViewModel = new();
        MainViewModel = new();
        contentViewModel = MainViewModel;
    }

    public StatsViewModel StatsVM { get; }
    public PackageSelectorViewModel PackageSelectorViewModel { get; }
    public MainViewModel MainViewModel { get; }

    public void Update()
    {
        AddItemViewModel addItemViewModel = new();

        Observable.Merge(
            addItemViewModel.OkCommand,
            addItemViewModel.CancelCommand.Select(_ => (Modem?)null))
            .Take(1)
            .Subscribe(newItem =>
                {
                    if (newItem != null)
                    {
                        StatsVM.Items.Add(newItem);
                    }
                    ContentViewModel = StatsVM;
                });

        ContentViewModel = addItemViewModel;
    }
}

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
        StatsViewModel = new(SerialPort.GetPortNames().Select(x => new Modem() { AttachedTo = x }));
        MainViewModel = new();
        contentViewModel = MainViewModel;
    }

    public StatsViewModel StatsViewModel { get; }
    public MainViewModel MainViewModel { get; }
    public Package Package { get; private set; } = new();

    public void ShowPackageSelector()
    {
        var psVM = new PackageSelectorViewModel();

        Observable.Merge(
            psVM.Load,
            psVM.Cancel.Select(_ => (Package?)null))
            .Take(1)
            .Subscribe(item => 
            {
                if (item is not null)
                {
                    Package = item;
                }
                ContentViewModel = MainViewModel;
            });
        ContentViewModel = psVM;
    }

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
                        StatsViewModel.Items.Add(newItem);
                    }
                    ContentViewModel = StatsViewModel;
                });

        ContentViewModel = addItemViewModel;
    }
}

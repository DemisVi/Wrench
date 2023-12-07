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

    public MainWindowViewModel()
    {
        MainViewModel = new();
        contentViewModel = MainViewModel;

        var enableSelector = this.WhenAny(x => x.MainViewModel.ControlViewModel.SelectedSource, y => y.Value is not null);

        ShowPackageSelector = ReactiveCommand.Create(ExecuteShowPackageSelector, enableSelector);
    }
    public ViewModelBase ContentViewModel
    {
        get => contentViewModel;
        private set => this.RaiseAndSetIfChanged(ref contentViewModel, value);
    }

    public ReactiveCommand<Unit, Unit> ShowPackageSelector { get; }
    
    public MainViewModel MainViewModel { get; }

    public void ExecuteShowPackageSelector()
    {
        var src = MainViewModel.ControlViewModel.SelectedSource;
        if (src is null) return;

        var psVM = new PackageSelectorViewModel(src);

        Observable.Merge(
            psVM.Load,
            psVM.Cancel.Select(_ => (Package?)null))
            .Take(1)
            .Subscribe(item =>
            {
                if (item is not null)
                {
                    MainViewModel.Package = item;
                }
                ContentViewModel = MainViewModel;
            });
        ContentViewModel = psVM;
    }
}

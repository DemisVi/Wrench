using System;
using System.IO.Ports;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using ReactiveUI;
using Wrench.Models;

namespace Wrench.ViewModels;

public class MainWindowViewModel : ViewModelBase, IDisposable
{
    private ViewModelBase contentViewModel;
    private bool disposedValue;

    public MainWindowViewModel()
    {
        MainViewModel = new();
        contentViewModel = MainViewModel;
#if DEBUG
        ShowPackageSelector = ReactiveCommand.Create(ExecuteShowPackageSelector);
#else
        var enableSelector = this.WhenAnyValue(x => x.MainViewModel.ControlViewModel.SelectedSource,
                                          y => y.MainViewModel.IsFlasherRunning,
                                          (x, y) => x is not null && y is not true);

        ShowPackageSelector = ReactiveCommand.Create(ExecuteShowPackageSelector, enableSelector);
#endif
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

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                // TODO: dispose managed state (managed objects)
                ShowPackageSelector.Dispose();
                MainViewModel.Dispose();
            }

            // TODO: free unmanaged resources (unmanaged objects) and override finalizer
            // TODO: set large fields to null
            disposedValue = true;
        }
    }

    // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
    // ~MainWindowViewModel()
    // {
    //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
    //     Dispose(disposing: false);
    // }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}

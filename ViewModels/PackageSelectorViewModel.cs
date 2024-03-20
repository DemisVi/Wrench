using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Reactive;
using ReactiveUI;
using Wrench.Models;
using Wrench.Services;

namespace Wrench.ViewModels;

public class PackageSelectorViewModel : ViewModelBase, IDisposable
{
    private Firmware? selectedFirmware;
    private ObservableCollection<Firmware>? firmwarePackages;
    private Package? selectedPackage;
    private FirmwareSource source;
    private bool disposedValue;

    public PackageSelectorViewModel(FirmwareSource source)
    {
        this.source = source;
        Refresh();

        var loadEnable = this.WhenAnyValue(x => x.SelectedPackage,
                                           x => x.SelectedFirmware,
                                           (x, y) => x is not null && y is not null);

        Load = ReactiveCommand.Create(() => SelectedPackage, loadEnable);
        Cancel = ReactiveCommand.Create(() => { });
    }

    public ObservableCollection<Firmware>? FirmwarePackages
    {
        get => firmwarePackages;
        set => this.RaiseAndSetIfChanged(ref firmwarePackages, value);
    }
    public Firmware? SelectedFirmware
    {
        get => selectedFirmware;
        set => this.RaiseAndSetIfChanged(ref selectedFirmware, value);
    }
    public Package? SelectedPackage
    {
        get => selectedPackage;
        set => this.RaiseAndSetIfChanged(ref selectedPackage, value);
    }

    public ReactiveCommand<Unit, Package?> Load { get; }
    public ReactiveCommand<Unit, Unit> Cancel { get; }

    public void Refresh() => FirmwarePackages = new(new FirmwareProvider().GetFirmware(source));

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                // TODO: dispose managed state (managed objects)
                Load.Dispose();
                Cancel.Dispose();
            }

            // TODO: free unmanaged resources (unmanaged objects) and override finalizer
            // TODO: set large fields to null
            disposedValue = true;
        }
    }

    // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
    // ~PackageSelectorViewModel()
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

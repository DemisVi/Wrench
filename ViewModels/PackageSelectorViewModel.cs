using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Reactive;
using ReactiveUI;
using Wrench.Models;
using Wrench.Services;

namespace Wrench.ViewModels;

public class PackageSelectorViewModel : ViewModelBase
{
    private Firmware? selectedFirmware;
    private ObservableCollection<Firmware>? firmwarePackages;
    private Package? selectedPackage;
    private FirmwareSource source;

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
}

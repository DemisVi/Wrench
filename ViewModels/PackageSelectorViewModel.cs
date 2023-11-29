using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using ReactiveUI;
using Wrench.Models;
using Wrench.Services;

namespace Wrench.ViewModels;

public class PackageSelectorViewModel : ViewModelBase
{
    private Firmware? selectedFirmware;
    private ObservableCollection<Firmware>? firmwarePackages;

    public PackageSelectorViewModel()
    {
        Refresh();
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
    public Package? SelectedPackage { get; set; }

    public void Refresh() => FirmwarePackages = new(new FirmwareProvider().GetFirmware(Path.Combine(Environment.CurrentDirectory, "./SimCom_retro")));
}

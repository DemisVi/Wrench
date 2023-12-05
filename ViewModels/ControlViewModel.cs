using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Avalonia.Controls;
using Wrench.Models;
using Wrench.Services;

namespace Wrench.ViewModels;

public class ControlViewModel : ViewModelBase
{
    public bool AllowPackageSelector { get; set; } = true;
    public IEnumerable<FirmwareSource> FirmwareSources { get; set; } = new FirmwareSourcesProvider().GetSources();
    public FirmwareSource? SelectedFirmwareItem { get; set; }
}

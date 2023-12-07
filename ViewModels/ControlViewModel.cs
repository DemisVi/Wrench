﻿using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Avalonia.Controls;
using ReactiveUI;
using Wrench.Models;
using Wrench.Services;

namespace Wrench.ViewModels;

public class ControlViewModel : ViewModelBase
{
    private FirmwareSource? selectedSource;

    public bool AllowPackageSelector { get; set; } = true;
    public IEnumerable<FirmwareSource> FirmwareSources { get; set; } = new FirmwareSourcesProvider().GetSources();
    public FirmwareSource? SelectedSource { get => selectedSource; set => this.RaiseAndSetIfChanged(ref selectedSource, value); }
}
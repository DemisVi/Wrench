using System;
using Avalonia.Media;
using Wrench.ViewModels;

namespace Wrench.ViewModels;

public class StatusViewModel : ViewModelBase
{
    public IBrush? IndicatorColor { get; set; }
    public string Label { get; set; } = "label";
}

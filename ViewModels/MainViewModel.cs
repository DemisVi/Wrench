using System;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using Avalonia.Media;
using DynamicData;
using ReactiveUI;
using Wrench.ViewModels;

namespace Wrench.ViewModels;

public class MainViewModel : ViewModelBase
{
    public ControlViewModel ControlViewModel { get; set; } = new();
}

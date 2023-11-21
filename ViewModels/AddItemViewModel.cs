using System;
using System.Reactive;
using ReactiveUI;
using Wrench.Models;

namespace Wrench.ViewModels;

public class AddItemViewModel : ViewModelBase
{
    private string description = string.Empty;

    public ReactiveCommand<Unit, Modem> OkCommand { get; }
    public ReactiveCommand<Unit, Unit> CancelCommand { get; }

    public AddItemViewModel()
    {
        var isValidObservable = this.WhenAnyValue(
            x => x.Description,
            x => !string.IsNullOrWhiteSpace(x));

        OkCommand = ReactiveCommand.Create(
            () => new Modem { AttachedTo = Description }, isValidObservable);
        CancelCommand = ReactiveCommand.Create(() => { });
    }
    public string Description
    {
        get => description;
        set => this.RaiseAndSetIfChanged(ref description, value);
    }
}

using System;
using System.Threading;
using ReactiveUI;

namespace Wrench.ViewModels;

public class StatusViewModel : ViewModelBase
{
    private TimeSpan elapsed;
    private Timer timeUpdater;
    private DateTime? currentTime;
    private string serialNumber = string.Empty;
    private int good;
    private int bad;
    private double coefficient;

    public StatusViewModel()
    {
        timeUpdater = new((_) => CurrentTime = DateTime.Now, null, 0, 1000);
        this.WhenAnyValue(x => x.Good, y => y.Bad).Subscribe(x => Coefficient = (double)x.Item1 / ((double)x.Item1 + (double)x.Item2));
    }
    public string Label { get; set; } = "p-holder";
    public string ContactUnit { get; set; } = string.Empty;
    public string SerialNumber { get => serialNumber; set => this.RaiseAndSetIfChanged(ref serialNumber, value); }
    public TimeSpan Elapsed { get => elapsed; set => this.RaiseAndSetIfChanged(ref elapsed, value); }
    public DateTime? CurrentTime { get => currentTime; set => this.RaiseAndSetIfChanged(ref currentTime, value); }
    public int Good { get => good; set => this.RaiseAndSetIfChanged(ref good, value); }
    public int Bad { get => bad; set => this.RaiseAndSetIfChanged(ref bad, value); }
    public double Coefficient { get => coefficient; set => this.RaiseAndSetIfChanged(ref coefficient, value); }
}

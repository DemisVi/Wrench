using System;
using System.Linq.Expressions;
using System.Reactive.Linq;
using System.Threading;
using ReactiveUI;
using Avalonia.Media;
using Wrench.DataTypes;

namespace Wrench.ViewModels;

public class StatusViewModel : ViewModelBase
{
    private TimeSpan elapsed;
    private DateTime? currentTime;
    private string serialNumber = string.Empty;
    private int good;
    private int bad;
    private double coefficient;
    private IImmutableSolidColorBrush statusColor = Brushes.Azure;
    private TimeSpan totalTime;
    private Timer clockTimer;

    public StatusViewModel()
    {
        clockTimer = new Timer((_) => CurrentTime = DateTime.Now, null, 0, 1000);
        this.WhenAnyValue(x => x.Good, y => y.Bad).Subscribe(x => Coefficient = (double)x.Item1 / ((double)x.Item1 + (double)x.Item2));
        this.WhenAnyValue(x => x.Good).Subscribe(x => { TotalTime += Elapsed; this.RaisePropertyChanged(nameof(TimePerBlock)); });
        this.WhenAnyValue(x => x.Good).Subscribe(x => StatusColor = Brushes.LightGreen);
        this.WhenAnyValue(x => x.Bad).Subscribe(x => StatusColor = Brushes.Pink);
    }

    public string Department => Constants.DepartmentName;
    public string ContactUnit { get; set; } = string.Empty;
    public string SerialNumber { get => serialNumber; set => this.RaiseAndSetIfChanged(ref serialNumber, value); }
    public TimeSpan Elapsed { get => elapsed; set => this.RaiseAndSetIfChanged(ref elapsed, value); }
    public TimeSpan TimePerBlock { get { try { return TotalTime / Good; } catch { return TimeSpan.Zero; } } }
    public TimeSpan TotalTime { get => totalTime; set => this.RaiseAndSetIfChanged(ref totalTime, value); }
    public DateTime? CurrentTime { get => currentTime; set => this.RaiseAndSetIfChanged(ref currentTime, value); }
    public int Good { get => good; set => this.RaiseAndSetIfChanged(ref good, value); }
    public int Bad { get => bad; set => this.RaiseAndSetIfChanged(ref bad, value); }
    public double Coefficient { get => coefficient; set => this.RaiseAndSetIfChanged(ref coefficient, value); }
    public IImmutableSolidColorBrush StatusColor { get => statusColor; set => this.RaiseAndSetIfChanged(ref statusColor, value); }
}

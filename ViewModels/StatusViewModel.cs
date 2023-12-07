using System;
using System.Threading.Tasks;
using System.Threading;
using Avalonia.Media;
using Iot.Device.Nmea0183.Sentences;
using ReactiveUI;
using Wrench.Models;
using Wrench.ViewModels;
using Iot.Device.Rfid;

namespace Wrench.ViewModels;

public class StatusViewModel : ViewModelBase
{
    private TimeSpan elapsed;
    private Timer timeUpdater;
    private DateTime? currentTime;

    public StatusViewModel()
    {
        timeUpdater = new((_) => CurrentTime = DateTime.Now, null, 0, 1000);
    }
    public string Label { get; set; } = "p-holder";
    public string ContactUnit { get; set; } = string.Empty;
    public string SerialNumber { get; set; } = string.Empty;
    public TimeSpan Elapsed { get => elapsed; set => this.RaiseAndSetIfChanged(ref elapsed, value); }
    public DateTime? CurrentTime { get => currentTime; set => this.RaiseAndSetIfChanged(ref currentTime, value); }
    public int Good { get; set; }
    public int Bad { get; set; }
}

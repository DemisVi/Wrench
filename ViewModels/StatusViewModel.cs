using System;
using System.Threading.Tasks;
using Avalonia.Media;
using ReactiveUI;
using Wrench.Models;
using Wrench.ViewModels;

namespace Wrench.ViewModels;

public class StatusViewModel : ViewModelBase
{
    private TimeSpan elapsed;

    public StatusViewModel()
    {
        Task.Factory.StartNew(() => 
        {
            while (true)
            {
                Elapsed += TimeSpan.FromSeconds(1);
                Task.Delay(1000).Wait();
            }
        });
    }
    public string Label { get; set; } = "p-holder";
    public string ContactUnit { get; set; } = string.Empty;
    public string SerialNumber { get; set; } = string.Empty;
    public TimeSpan Elapsed { get => elapsed; set => this.RaiseAndSetIfChanged(ref elapsed, value); }
    public int Good { get; set; }
    public int Bad { get; set; }
}

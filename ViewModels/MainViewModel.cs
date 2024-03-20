using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using Avalonia.Media;
using DynamicData;
using Iot.Device.FtCommon;
using ReactiveUI;
using Wrench.Models;
using Wrench.Services;
using Wrench.ViewModels;
using Wrench.Views;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Avalonia.Threading;
using System.Threading;
using Wrench.DataTypes;
using System.IO;
using System.Text.Json;
using System.Net.Http;
using Avalonia;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Wrench.Extensions;
using Iot.Device.Rfid;

namespace Wrench.ViewModels;

public class MainViewModel : ViewModelBase, IDisposable
{
    private Package? package;
    private CancellationTokenSource? cts;
    private bool isFlasherRunning;
    private bool disposedValue;
    private const string baseFirmwarePrefix = "./base";
    private const int BootUpTimeout = 15,
        ADBSwitchTimeout = 10,
        BootloaderRebootTimeout = 40;

    public bool IsFlasherRunning { get => isFlasherRunning; private set => this.RaiseAndSetIfChanged(ref isFlasherRunning, value); }
    public ControlViewModel ControlViewModel { get; set; } = new();
    public StatusViewModel StatusViewModel { get; set; } = new();
    public LogViewModel LogViewModel { get; set; } = new();
    public ReactiveCommand<Unit, Unit> FireTool { get; set; }
    public Package? Package { get => package; set => this.RaiseAndSetIfChanged(ref package, value); }



    public MainViewModel()
    {
        var fts = Ftx232HDevice.GetFtx232H();
        if (fts is { Count: > 0 })
            StatusViewModel.ContactUnit = fts.First().SerialNumber.TrimEnd('A', 'B');
#if DEBUG
        FireTool = ReactiveCommand.Create(PerformFireTool);
#else
        FireTool = ReactiveCommand.Create(PerformFireTool, this.WhenAnyValue(x => x.Package,
                                                                             y => y.IsFlasherRunning,
                                                                             (x, y) => (Package?)x is not null && y is false));
#endif
        this.WhenAnyValue(x => x.Package).Subscribe(x => StatusViewModel.SerialNumber = ReadSerial(x));
    }

    private string ReadSerial(Package? x)
    {
        if (x is null) return string.Empty;
        try
        {
            var fac = new FactoryCFG(Path.GetDirectoryName(x.PackagePath));
            fac.ReadFactory();
            var base34serial = fac.SerialNumber.ToString();
            return $"{base34serial} ({base34serial.ToInt32()})";
        }
        catch (FileNotFoundException)
        {
            return string.Empty;
        }
    }

    public void PerformFireTool()
    {

        var flasher = Package?.DeviceType switch
        {
            DeviceType.SimComFull or DeviceType.SimComRetro or DeviceType.SimComSimple => new Flasher(),
            DeviceType.SimComTechno => new TechnolabsFlasher(),
            _ => null,
        };
        cts = new CancellationTokenSource();
        var controller = new FlasherController()
        {
            Cts = cts,
            Package = Package,
            Flasher = flasher,
        };

        controller.EventOccurred += ResolveFlasherControllerEvents;

        controller.RunFlasher();
    }

    private void ResolveFlasherControllerEvents(object sender, FlasherControllerEventArgs e)
    {
        Action execute = e.EventType switch
        {
            FlasherControllerEventType.ProgressTimerElapsed => ProgressTimerElapsedHandler,
            FlasherControllerEventType.ProgressTimerReset => ProgressTimerResetHandler,
            FlasherControllerEventType.SuccessState => SuccessStateHandler,
            FlasherControllerEventType.FailState => FailStateHandler,
            FlasherControllerEventType.SignalReady => SignalReadyHandler,
            FlasherControllerEventType.SignalBusy => SignalBusyHandler,
            FlasherControllerEventType.LogMessage => delegate () { LogMessageHandler(e); }
            ,
            FlasherControllerEventType.FlasherStateChanged => delegate () { FlasherStateChangedHandler(e); }
            ,
            _ => delegate () { return; }
            ,
        };

        execute.Invoke();
    }

    private void ProgressTimerElapsedHandler() => Dispatcher.UIThread.Invoke(() => StatusViewModel.Elapsed += TimeSpan.FromSeconds(1));
    private void ProgressTimerResetHandler() => Dispatcher.UIThread.Invoke(() => StatusViewModel.Elapsed = TimeSpan.Zero);
    private void SuccessStateHandler() => Dispatcher.UIThread.Invoke(() =>
    {
        StatusViewModel.SerialNumber = ReadSerial(Package);
        StatusViewModel.Good++;
    });
    private void FailStateHandler() => Dispatcher.UIThread.Invoke(() => StatusViewModel.Bad++);
    private void SignalReadyHandler() => Dispatcher.UIThread.Invoke(() => StatusViewModel.StatusColor = Brushes.LightYellow);
    private void SignalBusyHandler() => Dispatcher.UIThread.Invoke(() => StatusViewModel.StatusColor = Brushes.LightBlue);
    private void LogMessageHandler(FlasherControllerEventArgs e) => Dispatcher.UIThread.Invoke(() => LogViewModel.Log.Add(e.Payload as string ?? ""));
    private void FlasherStateChangedHandler(FlasherControllerEventArgs e) => Dispatcher.UIThread.Invoke(() => IsFlasherRunning = (bool)e.Payload!);

    public void PerformCancel()
    {
        cts?.Cancel();
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                // TODO: dispose managed state (managed objects)
                cts?.Dispose();
                FireTool.Dispose();
            }

            // TODO: free unmanaged resources (unmanaged objects) and override finalizer
            // TODO: set large fields to null
            disposedValue = true;
        }
    }

    // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
    // ~MainViewModel()
    // {
    //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
    //     Dispose(disposing: false);
    // }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}

internal class TechnolabsFlasher : Flasher
{
}
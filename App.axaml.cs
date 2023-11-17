using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Wrench.ViewModels;
using Wrench.Views;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace Wrench;

public partial class App : Application
{
    private readonly IHost host = Host.CreateDefaultBuilder()
                     .ConfigureServices(s => s.AddSingleton<ViewModelBase, MainWindowViewModel>()
                                              .AddSingleton<MainWindow>()
                                              )
                     .Build();
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = host.Services.GetRequiredService<MainWindow>();
        }
    
        base.OnFrameworkInitializationCompleted();
    }
}

using System.IO.Ports;

namespace Wrench.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    public MainWindowViewModel(Greeter greeter)
    {
        Greeting = greeter.Greet;
    }
    public string Greeting { get; set; }
}

public class Greeter
{
    public string Greet { get; } = "Hello from DI";
}
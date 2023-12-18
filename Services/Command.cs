using System;
using System.Runtime.CompilerServices;

namespace Wrench.Services;

public class Command
{
    public void Execute([CallerMemberName] string name = "", [CallerLineNumber] int line = 0)
    {
        System.Console.WriteLine(name, line);
    }
}

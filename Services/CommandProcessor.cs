using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Wrench.Services;

public class CommandProcessor
{
    public CommandProcessor() 
    { 

    }
    public CommandProcessor(ICommand command) : this(new List<ICommand> { command })
    {

    }
    public CommandProcessor(IEnumerable<ICommand> commands) => Commands = commands;
    public IEnumerable<ICommand>? Commands { get; set; }

    public void Run([CallerMemberName] string name = "", [CallerLineNumber] int line = 0)
    {
        System.Console.WriteLine(name, line);
    }
}

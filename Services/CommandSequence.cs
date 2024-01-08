using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Wrench.Services;

public class CommandSequence
{
    private ConditionCommand canExecute = new ConditionCommand(delegate () { return true; });
    private readonly Action<string>? Log;

    // public CommandSequence() { }
    public CommandSequence(FlasherCommand command, ConditionCommand? canExecute = null, Action<string>? log = null)
         : this(new List<FlasherCommand> { command }, canExecute, log)
    { }
    public CommandSequence(IEnumerable<FlasherCommand> commands, ConditionCommand? canExecute = null, Action<string>? log = null)
    {
        Commands = commands;
        this.canExecute = canExecute ?? new(delegate () { return true; });
        Log = log;
    }

    public IEnumerable<FlasherCommand> Commands { get; set; }

    public void Run(CancellationToken? token = null)
    {
        // if (Commands is not null)
        if (canExecute.True)
            foreach (var c in Commands)
            {
                var resp = c.Execute();
                Log?.Invoke(string.Join(": ", resp.ResponceType, resp.ResponceMessage, c.CommandNote));

                if (token is not null and { IsCancellationRequested: true }) return;
            }
    }
}

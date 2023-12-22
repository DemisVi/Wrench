using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Wrench.Services;

public class CommandSequence
{
    private ConditionCommand canExecute = new ConditionCommand(delegate () { return true; });
    // public CommandSequence() { }
    public CommandSequence(FlasherCommand command, ConditionCommand? canExecute = null) : this(new List<FlasherCommand> { command }, canExecute) { }
    public CommandSequence(IEnumerable<FlasherCommand> commands, ConditionCommand? canExecute = null) =>
        (Commands, this.canExecute) = (commands, canExecute ?? new(delegate () { return true; }));

    public IEnumerable<FlasherCommand> Commands { get; set; }

    public void Run()
    {
        // if (Commands is not null)
        if (canExecute.True)
            foreach (var c in Commands)
                c.Execute();
    }
}

using System;

namespace Wrench.Services;

public class FlasherCommand
{
    private Action command;

    public FlasherCommand(Action command) => this.command = command;

    public void Execute() => command.Invoke();
}
using System;
using System.Runtime.CompilerServices;

namespace Wrench.Services;

public class ConditionCommand
{
    private readonly Func<bool> command;
    public ConditionCommand(Func<bool> action) => command = action;
    public bool True => command.Invoke();
}

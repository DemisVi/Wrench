using System;
using Wrench.Models;

namespace Wrench.Services;

public class FlasherCommand
{
    private Func<FlasherResponce> command;

    public FlasherCommand(Func<FlasherResponce> command) => this.command = command;

    public string CommandNote { get; set; } = string.Empty;

    public FlasherResponce Execute() => command.Invoke();
}
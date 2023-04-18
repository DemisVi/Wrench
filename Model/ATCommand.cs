using System;

namespace Wrench.Model;

public class ATCommand
{
    public string Command { get; set; } = string.Empty;
    public string Answer { get; set; } = string.Empty;

    public ATCommand() { }
    public ATCommand(string command, string answer = "OK")
    {
        if (string.IsNullOrEmpty(command)) throw new ArgumentException("Argument is null or empty", nameof(command));
        if (string.IsNullOrEmpty(answer)) throw new ArgumentException("Argument is null or empty", nameof(answer));

        Command = command;
        Answer = answer;
    }
}
using System;
using System.Diagnostics;

namespace Tools;

public abstract class Tool
{
    public abstract string ToolPath { get; }
    public virtual string LastStdOut { get; protected set; } = string.Empty;
    public virtual string LastStdErr { get; protected set; } = string.Empty;

    public virtual int Run(string command, int timeout = 2)
    {
        using var process = new Process()
        {
            StartInfo = new ProcessStartInfo(ToolPath)
            {
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                Arguments = command,
            },
        };

        process.Start();
        process.WaitForExit(TimeSpan.FromSeconds(timeout));

        if (!process.HasExited) process.Kill();

        LastStdErr = process.StandardError.ReadToEnd();
        LastStdOut = process.StandardOutput.ReadToEnd();

        return process.ExitCode;
    }
}

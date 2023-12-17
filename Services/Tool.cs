using System;
using System.ComponentModel;
using System.Diagnostics;

namespace Wrench.Services;

public abstract class Tool
{
    private int errorExitCode = -1;
    public abstract string ToolPath { get; }
    public virtual string LastStdOut { get; protected set; } = string.Empty;
    public virtual string LastStdErr { get; protected set; } = string.Empty;

    public virtual int Run(string command = "", int timeout = 2)
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

        try
        {
            process.Start();
            process.WaitForExit(TimeSpan.FromSeconds(timeout));

            if (!process.HasExited) process.Kill();

            LastStdErr = process.StandardError.ReadToEnd();
            LastStdOut = process.StandardOutput.ReadToEnd();
        }
        catch (Win32Exception ex)
        {
            LastStdErr = ex.Message;
            return errorExitCode;
        }
        catch (Exception ex)
        {
            LastStdErr = ex.Message;
            return errorExitCode;
        }

        return process.ExitCode;
    }
}

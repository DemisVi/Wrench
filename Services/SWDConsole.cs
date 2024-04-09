using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace Wrench.Services;

public class SWDConsole : Tool
{
    private IEnumerable<string> outputMessageFilter = new string[] {
        "successfully",
        "failed",
        "Enter into Fbf package",
        "Add an WTPTP device",
        // "USB device",
        "file name",
    };
    private string PercentageString { get; } = "percentage";
    private const int StdoutReadTimeout = 1000;
    private int errorExitCode = -1;
    private string progressZero = "0";
    public override string ToolPath { get; protected set; } = "SWDConsole";
    public Action<string>? Log { get; set; }
    public Action<int>? OnProgressChanged { get; set; }
    public override int Run(string command = "", int timeout = 2)
    {
        using var process = new Process()
        {
            StartInfo = new ProcessStartInfo(ToolPath)
            {
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                WorkingDirectory = WorkingDir,
                Arguments = command,
            },
        };

        process.OutputDataReceived += StdOutDataReceivedFilterHandler;
        // process.OutputDataReceived += (_, e) => System.Console.WriteLine(DateTime.Now.ToString("T") + e.Data);

        try
        {
            process.Start();
            process.BeginOutputReadLine();
            process.WaitForExit(TimeSpan.FromSeconds(timeout));

            Thread.Sleep(StdoutReadTimeout);

            process.CancelOutputRead();

            ProgressChanged(progressZero);

            if (!process.HasExited) process.Kill();

            LastStdErr = process.StandardError.ReadToEnd();
            // LastStdOut = process.StandardOutput.ReadToEnd();
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

    private void StdOutDataReceivedFilterHandler(object sender, DataReceivedEventArgs e)
    {
        foreach (var i in outputMessageFilter)
            if (e.Data is not null && e.Data.Contains(i))
                Log?.Invoke(e.Data);

        if (e.Data is not null && e.Data.Contains(PercentageString)) ProgressChanged(e.Data);
    }

    private void ProgressChanged(string processOutput)
    {
        if (OnProgressChanged is not null && int.TryParse(
                processOutput.Split(
                    " ",
                    StringSplitOptions.RemoveEmptyEntries |
                    StringSplitOptions.TrimEntries).LastOrDefault(), out var progress))
            OnProgressChanged.Invoke(progress);
    }
}

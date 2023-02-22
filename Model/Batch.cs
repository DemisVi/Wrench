using System;
using System.Diagnostics;
using System.IO;

namespace Wrench.Model;
public class Batch
{
    private const int millisMultiplier = 1000;
    public string? BatchPath { get; set; } = string.Empty;
    public string? Cwd { get; set; } = string.Empty;
    //public string? LastStdOut { get; private set; } = string.Empty;
    //public string? LastStdErr { get; private set; } = string.Empty;
    public ExitCodes? ExitCode { get; set; }

    public Batch(string batchPath, string currentWorkingDirektory)
    {
        BatchPath = batchPath;
        Cwd = currentWorkingDirektory;
    }

    public ExitCodes? Run(string? args = null, int timeOut = 100)
    {
        if (string.IsNullOrEmpty(BatchPath) || !File.Exists(BatchPath))
            throw new InvalidOperationException("Batch Path is invalid");

        using Process process = new()
        {
            StartInfo = new ProcessStartInfo()
            {
                FileName = BatchPath,
                Arguments = args,
                WorkingDirectory = Cwd,
                //RedirectStandardError = true,
                //RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            },
        };

        //process.OutputDataReceived += Process_OutputDataReceived;
        //process.ErrorDataReceived += Process_ErrorDataReceived;

        process.Start();
        //process.BeginOutputReadLine();
        //process.BeginErrorReadLine();

        process.WaitForExit(timeOut * millisMultiplier);

        if (process.HasExited) ExitCode = (ExitCodes)process.ExitCode;
        else if (!process.HasExited)
        {
            process.Kill();
            ExitCode = ExitCodes.UserTimeout;
        }

        //process.CancelOutputRead();
        //process.CancelErrorRead();

        return ExitCode;
    }

    private void Process_ErrorDataReceived(object sender, DataReceivedEventArgs e)
    {
        File.AppendAllText(Path.Combine(Environment.CurrentDirectory, "errorlog.log"),
            DateTime.Now.ToString("g") + Environment.NewLine + e.Data + Environment.NewLine);
    }

    private void Process_OutputDataReceived(object sender, DataReceivedEventArgs e)
    {
        File.AppendAllText(Path.Combine(Environment.CurrentDirectory, "outputlog.log"),
            DateTime.Now.ToString("g") + Environment.NewLine + e.Data + Environment.NewLine);
    }
}
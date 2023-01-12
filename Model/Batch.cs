using System;
using System.Diagnostics;
using System.IO;

namespace Wrench.Model;
public class Batch
{
    public string? BatchPath { get; set; } = string.Empty;
    public string? Cwd { get; set; } = string.Empty;
    public string? LastStdOut { get; private set; } = string.Empty;
    public string? LastStdErr { get; private set; } = string.Empty;

    public Batch(string batchPath, string currentWorkingDirektory)
    {
        BatchPath = batchPath;
        Cwd = currentWorkingDirektory;
    }

    public string Run(string? args = null, int timeOut = 2000, bool throwError = false)
    {
        if (string.IsNullOrEmpty(BatchPath) || !File.Exists(BatchPath))
            throw new InvalidOperationException("Batch Path is invalid");

        var startInfo = new ProcessStartInfo()
        {
            FileName = BatchPath,
            Arguments = args,
            WorkingDirectory = Cwd,
            RedirectStandardError = true,
            RedirectStandardOutput = true,
            UseShellExecute = false,
            CreateNoWindow = true,
        };
        using Process process = Process.Start(startInfo)!;
        LastStdOut = process.StandardOutput.ReadToEnd();
        LastStdErr = process.StandardError.ReadToEnd();
        process?.WaitForExit();
        if (!string.IsNullOrEmpty(LastStdErr) && throwError)
            throw new InvalidOperationException(LastStdErr);

        return LastStdOut;
    }
}
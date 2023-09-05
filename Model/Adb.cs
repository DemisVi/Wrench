using System;
using System.Diagnostics;
using System.IO;

namespace Wrench.Model;
public class Adb
{
    private const string _adbExe = "adb.exe";
    public string? AdbPath { get; set; } = string.Empty;
    // public string? LastStdOut { get; private set; } = string.Empty;
    // public string? LastStdErr { get; private set; } = string.Empty;
    public ExitCodes ExitCode { get; set; } = ExitCodes.WasNotRan;

    public Adb(string adbPath = "") =>
        AdbPath = adbPath.EndsWith(_adbExe) ? adbPath :
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                     "platform-tools",
                     _adbExe);

    public ExitCodes Run(string args, int timeOut = 2000, bool throwError = false)
    {
        if (string.IsNullOrEmpty(AdbPath) || !File.Exists(AdbPath))
            throw new InvalidOperationException("ADB Path is invalid");

        using Process process = new()
        {
            StartInfo = new ProcessStartInfo()
            {
                FileName = AdbPath,
                Arguments = args,
                // RedirectStandardError = true,
                // RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            }
        };

        process.Start();
        // LastStdOut = process.StandardOutput.ReadToEnd();
        // LastStdErr = process.StandardError.ReadToEnd();

        process.WaitForExit(timeOut * 1000);
        if (process.HasExited) ExitCode = (ExitCodes)process.ExitCode;
        else if (!process.HasExited)
        {
            process.Kill();
            ExitCode = ExitCodes.UserTimeout;
        }

        return ExitCode;
    }
}
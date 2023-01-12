using System;
using System.Diagnostics;
using System.IO;

namespace Wrench.Model;
public class Adb
{
private const string _adbExe = "adb.exe";
public string? AdbPath { get; set; } = string.Empty;
public string? LastStdOut { get; private set; } = string.Empty;
public string? LastStdErr { get; private set; } = string.Empty;

public Adb(string adbPath = "") =>
    AdbPath = adbPath.EndsWith(_adbExe) ? adbPath :
    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                 "platform-tools",
                 _adbExe);

public string? Run(string args, int timeOut = 2000, bool throwError = false)
{
    if (string.IsNullOrEmpty(AdbPath) || !File.Exists(AdbPath))
        throw new InvalidOperationException("ADB Path is invalid");

    var startInfo = new ProcessStartInfo()
    {
        FileName = AdbPath,
        Arguments = args,
        RedirectStandardError = true,
        RedirectStandardOutput = true,
        UseShellExecute = false,
        CreateNoWindow = true,
    };
    using var process = Process.Start(startInfo);
    LastStdOut = process?.StandardOutput.ReadToEnd();
    LastStdErr = process?.StandardError.ReadToEnd();
    process?.WaitForExit();
    if (!string.IsNullOrEmpty(LastStdErr) && throwError)
        throw new InvalidOperationException(LastStdErr);

    return LastStdOut;
}
}
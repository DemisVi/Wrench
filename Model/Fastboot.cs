using System;
using System.Diagnostics;
using System.IO;

public class Fastboot
{
    private const string _fbExe = "fastboot.exe";
    public string? FastbootPath { get; set; } = string.Empty;
    public string? LastStdOut { get; private set; } = string.Empty;
    public string? LastStdErr { get; private set; } = string.Empty;

    public Fastboot(string fbPath = "") =>
        FastbootPath = fbPath.EndsWith(_fbExe) ? fbPath :
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                     "platform-tools",
                     _fbExe);

    public string? Run(string args, int timeOut = 2000, bool throwError = false)
    {
        if (string.IsNullOrEmpty(FastbootPath) || !File.Exists(FastbootPath))
            throw new InvalidOperationException("Fastboot Path is invalid");

        var startInfo = new ProcessStartInfo()
        {
            FileName = FastbootPath,
            Arguments = args,
            RedirectStandardError = true,
            RedirectStandardOutput = true,
            UseShellExecute = false,
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
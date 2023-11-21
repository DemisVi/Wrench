using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Wrench.Models;

namespace Wrench.Services;

public class FirmwareProvider
{
    public string RootPath { get; set; } = string.Empty;
    public FirmwareProvider(string path)
    {
        RootPath = path;
    }

    public IEnumerable<Firmware> GetFirmware() => GetFirmware(RootPath);
    public static IEnumerable<Firmware> GetFirmware(string path)
    {
        if (string.IsNullOrEmpty(path)) throw new ArgumentException("Path can't be null or empty", nameof(path));

        var firmwareDirectories = Directory.GetDirectories(path);
        return firmwareDirectories.Select(dir => new Firmware()
        {
            FirmwarePath = dir,
            ModelName = new DirectoryInfo(dir).Name,
            Packages = Directory.GetDirectories(dir).Select(dir => new Package()
            {
                PackagePath = dir,
                VersionName = new DirectoryInfo(dir).Name,
            }),
        });
    }
}

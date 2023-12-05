using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Wrench.Models;

namespace Wrench.Services;

public class FirmwareProvider : IFirmwareProvider
{
    public string RootPath { get; set; } = string.Empty;
    public FirmwareProvider() { }
    public FirmwareProvider(string path)
    {
        RootPath = path;
    }

    public IEnumerable<Firmware> GetFirmware() => GetFirmware(RootPath);
    public IEnumerable<Firmware> GetFirmware(string path)
    {
        if (string.IsNullOrEmpty(path)) throw new ArgumentException("Path can't be null or empty", nameof(path));

        if (Directory.Exists(path) is not true)
            return Enumerable.Empty<Firmware>();

        var firmwareDirectories = Directory.GetDirectories(path);
        return firmwareDirectories.Select(dir => new Firmware()
        {
            FirmwarePath = dir,
            ModelName = new DirectoryInfo(dir).Name,
            Packages = Directory.GetDirectories(dir).Select(indir => new Package()
            {
                ModelName = new DirectoryInfo(dir).Name,
                PackagePath = indir,
                VersionName = new DirectoryInfo(indir).Name,
            }),
        });
    }
}

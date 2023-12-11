using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Wrench.Models;

namespace Wrench.Services;

public class FirmwareProvider : IFirmwareProvider
{
    public FirmwareSource? Source { get; set; }
    public FirmwareProvider() { }
    public FirmwareProvider(FirmwareSource source)
    {
        Source = source;
    }

    public IEnumerable<Firmware> GetFirmware() => GetFirmware(Source);
    public IEnumerable<Firmware> GetFirmware(FirmwareSource? source)
    {
        if (source is null) throw new ArgumentException("Path can't be null or empty", nameof(source));

        var path = Path.Combine(Environment.CurrentDirectory, source.SubfolderName);

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
                DeviceType = source.DeviceType,
            }),
        });
    }
}

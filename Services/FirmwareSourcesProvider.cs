using System;
using System.Security;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using Wrench.Models;
using Wrench.DataTypes;

namespace Wrench.Services;

public class FirmwareSourcesProvider : IFirmwareSourcesProvider
{
    public static string DefaultFileName { get; } = "Sources.json";
    public string DefaultSourcesFile { get; set; } = Path.Combine(Environment.CurrentDirectory, DefaultFileName);
    private IEnumerable<FirmwareSource> DefaultSources { get; } = new FirmwareSource[] {
         new()
         {
            Name = "SimCom ПФ",
            SubfolderName = "./SimCom_full",
            DeviceType = DeviceType.SimComFull,
        },
        new() {
            Name = "SimCom Ретрофит",
            SubfolderName = "./SimCom_retro",
            DeviceType = DeviceType.SimComRetro,
        },
        new() {
            Name = "SimCom Упрощенный",
            SubfolderName = "./SimCom_simple",
            DeviceType = DeviceType.SimComSimple,
        },
        new() {
            Name = "Telit Ретрофит",
            SubfolderName = "./Telit_retro",
            DeviceType = DeviceType.TelitRetro,
        },
        new() {
            Name = "Telit Упрощенный",
            SubfolderName = "./Telit_simple",
            DeviceType = DeviceType.TelitSimple,
        }};

    public IEnumerable<FirmwareSource> GetSources() => GetSources(DefaultSourcesFile);
    public IEnumerable<FirmwareSource> GetSources(string path)
    {
        if (File.Exists(path) is not true)
        {
            var @default = JsonSerializer.Serialize(DefaultSources);
            File.WriteAllText(path, @default);
            return DefaultSources;
        }
        else
        {
            try
            {
                return JsonSerializer.Deserialize<FirmwareSource[]>(File.ReadAllText(path))!;
            }
            catch (JsonException ex)
            {
                return new FirmwareSource[] { new() { Name = ex.Message } };
            }
        }
    }
};

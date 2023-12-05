using System;
using System.Security;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using Wrench.Models;

namespace Wrench.Services;

public class FirmwareSourcesProvider : IFirmwareSourcesProvider
{
    public static string DefaultFileName { get; } = "Sources.json";
    public string SourceFile { get; set; } = Path.Combine(Environment.CurrentDirectory, DefaultFileName);
    private IEnumerable<FirmwareSource> DefaultSource { get; } = new FirmwareSource[] {
         new()
         {
            Name = "SimCom ПФ",
            SubfolderName = "./SimCom_full"
        },
        new() {
            Name = "SimCom Ретрофит",
            SubfolderName = "./SimCom_retro"
        },
        new() {
            Name = "SimCom Упрощенный",
            SubfolderName = "./SimCom_simple"
        },
        new() {
            Name = "Telit Упрощенный",
            SubfolderName = "./Telit_simple"
        }};

    public IEnumerable<FirmwareSource> GetSources() => GetSources(SourceFile);
    public IEnumerable<FirmwareSource> GetSources(string path)
    {
        if (File.Exists(path) is not true)
        {
            var @default = JsonSerializer.Serialize(DefaultSource);
            File.WriteAllText(path, @default);
            return DefaultSource;
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

using System;
using System.Collections.Generic;
using Wrench.Models;

namespace Wrench.Services;

public interface IFirmwareSourcesProvider
{
    public IEnumerable<FirmwareSource> GetSources();
    public IEnumerable<FirmwareSource> GetSources(string path);
}
using System;
using System.Collections.Generic;

namespace Wrench.Models;

public class Package
{
    public string ModelName { get; set; } = string.Empty;
    public string VersionName { get; set; } = string.Empty;
    public string PackagePath { get; set; } = string.Empty;
}

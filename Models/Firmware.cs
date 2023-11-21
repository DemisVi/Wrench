using System;
using System.Collections.Generic;

namespace Wrench.Models;

public class Firmware
{
    public string ModelName { get; set; } = string.Empty;
    public string FirmwarePath { get; set; } = string.Empty;
    public IEnumerable<Package>? Packages { get; set; }
}

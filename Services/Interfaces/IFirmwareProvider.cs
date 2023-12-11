using System.Collections.Generic;
using Wrench.Models;
using Wrench.DataTypes;

namespace Wrench.Services
{
    public interface IFirmwareProvider
    {
        FirmwareSource? Source { get; set; }

        IEnumerable<Firmware> GetFirmware();
        IEnumerable<Firmware> GetFirmware(FirmwareSource? source);
    }
}

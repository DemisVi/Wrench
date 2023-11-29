using System.Collections.Generic;
using Wrench.Models;
using Wrench.Services;

namespace Wrench.Services
{
    public interface IFirmwareProvider
    {
        string RootPath { get; set; }

        IEnumerable<Firmware> GetFirmware();
        IEnumerable<Firmware> GetFirmware(string path);
    }
}

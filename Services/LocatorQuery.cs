using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;

#pragma warning disable CA1416 // Disable platform compatibility warning

namespace Wrench.Services
{
    public class LocatorQuery
    {
        public static readonly WqlEventQuery queryEventDevice = new("SELECT * FROM Win32_DeviceChangeEvent"
            + " WHERE EventType = 2 GROUP WITHIN 4");

        public static readonly WqlEventQuery queryEventTelit = new("SELECT * FROM __InstanceCreationEvent "
            + "WITHIN 2 WHERE "
            + "TargetInstance ISA 'Win32_POTSModem' "
            + "AND TargetInstance.Caption LIKE '%Telit%' "
            + "GROUP WITHIN 4");

        public static readonly WqlEventQuery queryEventSimcom = new("SELECT * FROM __InstanceCreationEvent "
            + "WITHIN 2 WHERE "
            + "TargetInstance ISA 'Win32_POTSModem' "
            + "AND TargetInstance.Caption LIKE '%Simcom%' "
            + "GROUP WITHIN 4");

        public static readonly WqlObjectQuery queryTelitModem = new("SELECT AttachedTo FROM Win32_POTSModem WHERE"
            + " Caption LIKE '%Telit%'");

        public static readonly WqlObjectQuery querySimcomModem = new("SELECT AttachedTo FROM Win32_POTSModem WHERE"
            + " Caption LIKE '%Simcom%'");
    }
}

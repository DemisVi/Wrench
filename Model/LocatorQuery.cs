using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;

#pragma warning disable CA1416 // Disable platform compatibility warning

namespace Wrench.Model;

public class LocatorQuery
{
    public static readonly WqlEventQuery queryEventDeviceChange = new("SELECT * FROM Win32_DeviceChangeEvent"
        + " WHERE EventType = 2 GROUP WITHIN 4");

    public static readonly WqlEventQuery queryEventTelit = new("SELECT * FROM __InstanceCreationEvent "
        + "WITHIN 1 WHERE "
        + "TargetInstance ISA 'Win32_POTSModem' "
        + "AND (TargetInstance.Caption LIKE '%Telit%' "
        + "OR TargetInstance.ProviderName LIKE '%Telit%' "
        + "OR TargetInstance.DeviceID LIKE '%VID_1BC7&PID_1201%') "
        + "GROUP WITHIN 4");

    public static readonly WqlEventQuery queryEventSimcom = new("SELECT * FROM __InstanceCreationEvent "
        + "WITHIN 1 WHERE "
        + "TargetInstance ISA 'Win32_POTSModem' "
        + "AND (TargetInstance.Caption LIKE '%Simcom%' "
        + "OR TargetInstance.ProviderName LIKE '%Simcom%' "
        + "OR TargetInstance.DeviceID LIKE '%VID_1E0E&PID_9001%') "
        + "GROUP WITHIN 4");

    public static readonly WqlEventQuery queryEventAndroid = new(
        "SELECT * FROM  __InstanceCreationEvent "
        + "WITHIN 1 WHERE "
        + "TargetInstance ISA 'Win32_PnpEntity' "
        + "AND TargetInstance.ClassGuid = '{3f966bd9-fa04-4ec5-991c-d326973b5128}'"
    );

    public static readonly WqlObjectQuery queryTelitModem = new("SELECT AttachedTo FROM Win32_POTSModem WHERE "
        + "(Caption LIKE '%Telit%' OR ProviderName LIKE '%Telit%' OR DeviceID LIKE '%VID_1BC7&PID_1201%')");

    public static readonly WqlObjectQuery querySimcomModem = new("SELECT AttachedTo FROM Win32_POTSModem WHERE "
        + "(Caption LIKE '%SimTech%' OR ProviderName LIKE '%SimTech%' OR DeviceID LIKE '%VID_1E0E&PID_9001%')");

    public static readonly WqlObjectQuery querySimcomATPort = new("SELECT Caption FROM Win32_PNPEntity WHERE "
        + "(Description LIKE '%USB AT%' AND DeviceID LIKE '%VID_1E0E&PID_9001%') ");

    public static readonly WqlEventQuery creationSimComADB = new WqlEventQuery(
        "SELECT * FROM __InstanceCreationEvent WITHIN 1 WHERE " +
        "TargetInstance ISA 'Win32_PnPEntity' " +
        "AND TargetInstance.ClassGuid = '{3f966bd9-fa04-4ec5-991c-d326973b5128}'");

    public static readonly WqlEventQuery deletionSimComADB = new WqlEventQuery(
        "SELECT * FROM __InstanceDeletionEvent WITHIN 1 WHERE " +
        "TargetInstance ISA 'Win32_PnPEntity' " +
        "AND TargetInstance.ClassGuid = '{3f966bd9-fa04-4ec5-991c-d326973b5128}'");

    public static readonly WqlObjectQuery androidDevice = new WqlObjectQuery(
        "SELECT * FROM Win32_PnpEntity WHERE ClassGuid = '{3f966bd9-fa04-4ec5-991c-d326973b5128}'");
}

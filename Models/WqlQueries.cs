using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;

public static class WqlQueries
{
#pragma warning disable CA1416

    public static WqlObjectQuery ObjectTelitModem { get; } = new(
        "SELECT AttachedTo FROM Win32_POTSModem WHERE "
        + "(Caption LIKE '%Telit%' OR ProviderName LIKE '%Telit%' OR DeviceID LIKE '%VID_1BC7&PID_1201%')");

    public static WqlObjectQuery ObjectSimcomModem { get; } = new(
        "SELECT AttachedTo FROM Win32_POTSModem WHERE "
        + "(Caption LIKE '%SimTech%' OR ProviderName LIKE '%SimTech%' OR DeviceID LIKE '%VID_1E0E&PID_9001%')");

    public static WqlObjectQuery ObjectSimcomATPort { get; } = new(
        "SELECT Caption FROM Win32_PNPEntity WHERE "
        + "(Description LIKE '%USB AT%' AND DeviceID LIKE '%VID_1E0E&PID_9001%') ");

    public static WqlObjectQuery ObjectAndroidDevice { get; } = new(
        "SELECT * FROM Win32_PnpEntity WHERE ClassGuid = '{3f966bd9-fa04-4ec5-991c-d326973b5128}'");

    public static WqlObjectQuery ObjectFtdiSerial { get; } = new(
        "SELECT Caption FROM Win32_PnPEntity WHERE Manufacturer = 'FTDI' AND Caption LIKE 'USB Serial Port (%)'");

    public static WqlEventQuery ChangeEventDevice { get; } = new(
        "SELECT * FROM Win32_DeviceChangeEvent"
        + " WHERE EventType = 2 GROUP WITHIN 4");

    public static WqlEventQuery CreationEventTelit { get; } = new(
        "SELECT * FROM __InstanceCreationEvent "
        + "WITHIN 1 WHERE "
        + "TargetInstance ISA 'Win32_POTSModem' "
        + "AND (TargetInstance.Caption LIKE '%Telit%' "
        + "OR TargetInstance.ProviderName LIKE '%Telit%' "
        + "OR TargetInstance.DeviceID LIKE '%VID_1BC7&PID_1201%') "
        + "GROUP WITHIN 4");

    public static WqlEventQuery CreationEventSimcom { get; } = new(
        "SELECT * FROM __InstanceCreationEvent "
        + "WITHIN 1 WHERE "
        + "TargetInstance ISA 'Win32_POTSModem' "
        + "AND (TargetInstance.Caption LIKE '%Simcom%' "
        + "OR TargetInstance.ProviderName LIKE '%Simcom%' "
        + "OR TargetInstance.DeviceID LIKE '%VID_1E0E&PID_9001%') "
        + "GROUP WITHIN 4");

    public static WqlEventQuery CreationEventAndroid { get; } = new(
        "SELECT * FROM  __InstanceCreationEvent "
        + "WITHIN 1 WHERE "
        + "TargetInstance ISA 'Win32_PnpEntity' "
        + "AND TargetInstance.ClassGuid = '{3f966bd9-fa04-4ec5-991c-d326973b5128}'");

    public static WqlEventQuery CreationSimComADB { get; } = new(
        "SELECT * FROM __InstanceCreationEvent WITHIN 1 WHERE " +
        "TargetInstance ISA 'Win32_PnPEntity' " +
        "AND TargetInstance.ClassGuid = '{3f966bd9-fa04-4ec5-991c-d326973b5128}'");

    public static WqlEventQuery DeletionSimComADB { get; } = new(
        "SELECT * FROM __InstanceDeletionEvent WITHIN 1 WHERE " +
        "TargetInstance ISA 'Win32_PnPEntity' " +
        "AND TargetInstance.ClassGuid = '{3f966bd9-fa04-4ec5-991c-d326973b5128}'");

#pragma warning restore CA1416
}

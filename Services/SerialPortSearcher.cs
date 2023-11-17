using System;
using System.Linq;
using System.Management;

namespace Wrench.Services;

#pragma warning disable CA1416

public class SerialPortSearcher
{
    public WqlObjectQuery Query { get; set; } = new WqlObjectQuery();
    public SerialPortSearcher() { }
    public SerialPortSearcher(WqlObjectQuery query) => Query = query;
    public string[] GetPortNames() => GetPortNames(Query);
    public static string[] GetPortNames(WqlObjectQuery query)
    {
        using var searcher = new ManagementObjectSearcher(query);
        var objs = searcher.Get();

        if (objs is { Count: > 0 })
            return objs.Cast<ManagementObject>().Select(x => GetSerialName(x)).ToArray();
        else
            return Array.Empty<string>();
    }

    private static string GetSerialName(ManagementObject x) => x.GetText(TextFormat.Mof).Split(
        new char[] { '(', ')', '"' },
        StringSplitOptions.RemoveEmptyEntries).First(x => x.StartsWith("COM"));
}

#pragma warning restore CA1416

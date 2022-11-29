using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System.Text;
using System.IO.Ports;
using System.Threading;
using System.Management;

using Wrench.Extensions;
using Wrench.Model;

#pragma warning disable CA1416 // Disable platform compatibility warning

namespace Wrench.Services;

public class ModemLocator
{
    public WqlEventQuery EventQuery { get; set; }
    public WqlObjectQuery DeviceQuery { get; set; }

    public ModemLocator(WqlEventQuery eventQuery, WqlObjectQuery objectQuery)
    {
        EventQuery = eventQuery;
        DeviceQuery = objectQuery;
    }

    public ManagementBaseObject WaitDeviceConnect()
    {
        using ManagementEventWatcher watcher = new(EventQuery);
        return watcher.WaitForNextEvent();
    }

    public async Task<List<Modem>> LocateDevicesAsync()
    {
        var modemInfo = new List<string[]>();

        foreach (var item in GetModemSerialPorts())
        {
            var tempStringBuilder = new StringBuilder();
            tempStringBuilder.AppendLine(item.PortName);

            if (item.IsOpen) { throw new InvalidOperationException($"SerialPort {item.PortName} opened somewhere else"); }
            if (!item.IsOpen) item.Open();
            await item.WaitModemStartAsync();

            item.WriteLine("AT+GSN=0");

            await Task.Delay(250);

            tempStringBuilder.Append(item.ReadExisting());

            item.Close();

            modemInfo.Add(tempStringBuilder.ToString().Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries));
        }

        return Distinct(modemInfo).GroupBy(x => x.SerialNumber).Select(x => x.First()).ToList<Modem>();
    }

    public List<Modem> LocateDevices() => LocateDevicesAsync().GetAwaiter().GetResult();

    public List<string> GetModemPortNames()
    {
        using var searcher = new ManagementObjectSearcher(DeviceQuery);

        var serials = new List<string>();

        serials.AddRange(searcher.Get().Cast<ManagementObject>().Select(x => x["AttachedTo"].ToString()!));

        return serials;
    }

    public List<SerialPort> GetModemSerialPorts()
    {
        using var searcher = new ManagementObjectSearcher(DeviceQuery);

        var serials = new List<SerialPort>();

        serials.AddRange(searcher.Get().Cast<ManagementObject>().Select(x => new SerialPort()
        {
            PortName = x["AttachedTo"].ToString()!,
            Handshake = Handshake.RequestToSend,
            NewLine = "\r",
            WriteTimeout = 1200,
            ReadTimeout = 1200,
            BaudRate = 115200,
        }));

        return serials;
    }

    private List<Modem> Distinct(List<string[]> modemInfo)
    {
        var res = new List<Modem>();

        foreach (var item in modemInfo)
            foreach (var inneritem in item)
                if (long.TryParse(inneritem, out var sn))
                    res.Add(new Modem() { AttachedTo = item.First(), SerialNumber = sn.ToString() });

        return res;
    }
}
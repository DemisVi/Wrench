using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO.Ports;
using System.Threading;
using System.Threading.Tasks;
using Wrench.Model;
using Wrench.Extensions;
using ROOT.CIMV2.Win32;
using Wrench.Services;

namespace Wrench.Services
{
    public class TelitLocator
    {
        private List<SerialPort> _serialsList = new List<SerialPort>();
        private List<string[]> _modemInfo = new List<string[]>();
        private List<Modem> _res = new List<Modem>();

        public async Task<List<Modem>> LocateDevicesAsync()
        {
            foreach (POTSModem item in POTSModem.GetInstances("ProviderName = 'Telit'"))
                _serialsList.Add(new SerialPort()
                {
                    PortName = item.AttachedTo,
                    Handshake = Handshake.RequestToSend,
                    NewLine = "\r",
                    WriteTimeout = 1200,
                    ReadTimeout = 1200,
                    BaudRate = 115200,
                });

            foreach (var item in _serialsList)
            {
                var tempStringBuilder = new StringBuilder();
                tempStringBuilder.AppendLine(item.PortName);

                if (!item.IsOpen) item.Open();
                await item.WaitModemStartAsync();

                item.WriteLine("AT+GSN=0");

                item.ReadLine();

                tempStringBuilder.Append(item.ReadExisting());

                item.Close();

                _modemInfo.Add(tempStringBuilder.ToString().Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries));
            }

            foreach (var item in _modemInfo)
                foreach (var inneritem in item)
                    if (long.TryParse(inneritem, out var sn))
                        _res.Add(new Modem() { AttachedTo = item.First(), Serial = sn.ToString() });

            return _res.DistinctBy(x => x.Serial).ToList();
        }

        public List<Modem> LocateDevices()
        {
            foreach (POTSModem item in POTSModem.GetInstances("ProviderName = 'Telit'"))
                _serialsList.Add(new SerialPort()
                {
                    PortName = item.AttachedTo,
                    Handshake = Handshake.RequestToSend,
                    NewLine = "\r",
                    WriteTimeout = 200,
                    ReadTimeout = 200,
                    BaudRate = 115200,
                });

            foreach (var item in _serialsList)
            {
                var tempStringBuilder = new StringBuilder();
                tempStringBuilder.AppendLine(item.PortName);

                if (!item.IsOpen) item.Open();
                item.WaitModemStart();

                item.WriteLine("AT+GSN=0");

                item.ReadLine();

                tempStringBuilder.Append(item.ReadExisting());

                item.Close();

                _modemInfo.Add(tempStringBuilder.ToString().Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries));
            }

            foreach (var item in _modemInfo)
                foreach (var inneritem in item)
                    if (long.TryParse(inneritem, out var sn))
                        _res.Add(new Modem() { AttachedTo = item.First(), Serial = sn.ToString() });

            return _res.DistinctBy(x => x.Serial).ToList();
        }
    }
}
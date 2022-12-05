using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using FTD2XX_NET;

namespace Wrench.Services
{
    public class AdapterLocator
    {
        private FTDI _ftdi = new FTDI();
        private uint _devCount = 0;
        private FTDI.FT_DEVICE_INFO_NODE[] _ftdiDeviceList = new FTDI.FT_DEVICE_INFO_NODE[0];

        public AdapterLocator() => Rescan();

        public List<string> AdapterSerials { get => _ftdiDeviceList.Select(x => x.SerialNumber).ToList(); }

        public uint AdapterCount => _devCount;

        public void Rescan()
        {
            _ftdi.GetNumberOfDevices(ref _devCount);
            _ftdiDeviceList = new FTDI.FT_DEVICE_INFO_NODE[_devCount];
            _ftdi.GetDeviceList(_ftdiDeviceList);
        }
    }
}

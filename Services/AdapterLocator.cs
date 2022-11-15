using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using FTD2XX_NET;

namespace Wrench.Services
{
    internal class AdapterLocator
    {
        private FTDI _ftdi = new FTDI();
        private uint _devCount = 0;
        private FTDI.FT_DEVICE_INFO_NODE[] _ftdiDeviceList;

        public AdapterLocator()
        {
            _ftdi.GetNumberOfDevices(ref _devCount);
            _ftdiDeviceList = new FTDI.FT_DEVICE_INFO_NODE[_devCount];
            _ftdi.GetDeviceList(_ftdiDeviceList);
        }

        public List<string> AdapterSerials { get { return _ftdiDeviceList.Select(x => x.SerialNumber).ToList(); } }

        //public List<string> AdapterSerials { get { return _ftdiDeviceList.Select(x => 
        //    Encoding.UTF8.GetString(Encoding.Convert(Encoding.ASCII, Encoding.UTF8,
        //    Encoding.ASCII.GetBytes(x.SerialNumber)))).ToList(); } }
        public uint AdapterCount => _devCount;
    }
}

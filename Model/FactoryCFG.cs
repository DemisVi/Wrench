using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wrench.Model
{
    internal class FactoryCFG
    {
        private const string _baseName = "factory.cfg";
        private readonly string _path;
        private Dictionary<string, string> _factory = new();
        public string ModelId => this["MODEL_ID"];
        public string SerialNumber
        {
            get => this["SERIAL_NUMBER"];
            set => this["SERIAL_NUMBER"] = value;
        }

        public FactoryCFG(string path = "./") => _path = Path.Combine(path, _baseName);

        public void ReadFactory()
        {
            var lines = File.ReadAllLines(_path);
            foreach (var l in lines)
            {
                var temp = l.Split('=');
                _factory.Add(temp.First(), temp.Last());
            }
        }

        public void SaveFactory()
        {
            var lines = _factory.Select(x => string.Join('=', x.Key, x.Value)).ToArray();
            File.WriteAllLines(_path, lines);
        }

        public string this[string parameter]
        {
            get => _factory[parameter];
            set => _factory[parameter] = value;
        }
    }
}

using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wrench.Extensions;
using Wrench.DataTypes;

namespace Wrench.Models;

public class FactoryCFG
{
    private const string _baseName = "factory.cfg";
    private readonly string _path;
    private Dictionary<string, string> _factory = new();
    public string ModelId
    {
        get => this["MODEL_ID"];
        set => this["MODEL_ID"] = value;
    }
    public Base34 SerialNumber
    {
        get => this["SERIAL_NUMBER"].ToBase34();
        set => this["SERIAL_NUMBER"] = value.ToDeviceSerial();
    }

    public FactoryCFG(string? path = "./") => _path = Path.Combine(path ?? Environment.CurrentDirectory, _baseName);

    public void ReadFactory()
    {
        try
        {
            var lines = File.ReadAllLines(_path);
            foreach (var l in lines)
            {
                var temp = l.Split('=');
                _factory.Add(temp.First(), temp.Last());
            }
        }
        catch (Exception) { throw; }
    }

    public void SaveFactory()
    {
        var lines = _factory.Select(x => string.Join('=', x.Key, x.Value)).ToArray();
        File.WriteAllLines(_path, lines);
    }

    public void UpdateFactory()
    {
        ReadFactory();
        SerialNumber++;
        SaveFactory();
    }

    public string this[string parameter]
    {
        get => _factory[parameter];
        set => _factory[parameter] = value;
    }
}

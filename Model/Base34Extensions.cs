using System;
using System.Linq;
using System.Text;

namespace Wrench.Model;

public static class Base34Extensions
{
    public static string ToDeviceSerial(this Base34 value) =>
        $"T7G84178{DateTime.Now.DayOfYear.ToString("D3")}{DateTime.Now.ToString("yy")}0000{value.ToString()}";

    public static Base34 ToBase34(this string value)
    {
        if (value.Contains("___")) return new Base34(string.Empty);
        if (!value.StartsWith("T7G84178", true, null) && !string.IsNullOrEmpty(value)) throw new ArgumentException("String is not in DeviceSerial format");
        if (value.Contains("000", StringComparison.Ordinal))
            return new Base34(value.Substring(value.IndexOf("000", StringComparison.Ordinal)));
        else return new Base34(string.Empty);
    }
    public static int ToInt32(this char value) => ToInt32(value.ToString());
    public static int ToInt32(this string value)
    {
        var serial = value.ToUpper().Reverse();
        int integer = 0;
        int power = 1;

        foreach (var c in serial)
        {
            int index = Array.IndexOf(Base34.Base, c);

            if (index == -1) throw new ArgumentException("Not a base-34 string");

            integer += index * power;
            power *= 34;
        }
        return integer;
    }

    public static string ToBase34(this int value, int rank = 4)
    {
        StringBuilder result = new();
        int targetBase = (int)Base34.Base.Length;

        do
        {
            result.Insert(0, Base34.Base[value % targetBase]);
            value = value / targetBase;
        }
        while (value > 0);

        if (rank > 0) result.Insert(0, "0", rank - result.Length);

        return result.ToString();
    }
}
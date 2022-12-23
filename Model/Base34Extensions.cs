using System;
using System.Text;

public static class Base34Extensions
{
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

    public static string ToBase34(this int value, int rank = 0)
    {
        StringBuilder result = new();
        int targetBase = (int)Base34.Base.Length;

        do
        {
            result.Insert(0, Base34.Base[value % targetBase]);
            value = value / targetBase;
        }
        while (value > 0);

        if (rank > 0) result.Insert(0, "0", rank);

        return result.ToString();
    }
}
using System;
using Wrench.Extensions;

namespace Wrench.DataTypes;

public class Base34
{
    private int _value = 0;
    private static readonly char[] base34 = new[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'J', 'K', 'L', 'M', 'N', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z' };
    public static char[] Base => base34;
    public Base34(string value) => _value = value.ToInt32();
    public Base34(int value) => _value = value;
    public override string ToString() => _value.ToBase34();
    
    public static Base34 operator ++(Base34 item)
    {
        item._value++;
        return item;
    }

    public static Base34 operator --(Base34 item)
    {
        item._value--;
        return item;
    }
    public static explicit operator string(Base34 base34) => base34.ToString();
    public static explicit operator Base34(string base34String) => new Base34(base34String);
}

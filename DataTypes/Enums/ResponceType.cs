using System;

namespace Wrench.DataTypes;

[Flags]
public enum ResponceType
{
    Unsuccess = -1,
    OK = 0,
    Fail = 1,
    Timeout = 2,
    NotFound = 4,
}

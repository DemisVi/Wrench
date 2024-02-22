using System;

namespace Wrench.DataTypes;

[Flags]
public enum ResponseType
{
    Info = -1,
    OK = 0,
    Fail = 1,
    Timeout = 2,
    NotFound = 4,
}

using System;

namespace Wrench.DataTypes;

[Flags]
public enum DeviceType
{
    Unknown = 0,
    SimComFull = 1,
    SimComRetro = 2,
    SimComSimple = 4,
    TelitRetro = 8,
    TelitSimple = 16,
}
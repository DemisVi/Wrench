using System;
using System.Diagnostics;

namespace Tools;

public class Adb : Tool
{
    public override string ToolPath { get; } = "adb";
}

using System;

public static class CuCommands
{
    public static byte[] CuWriteOutputs { get; } = new byte[] { 0xCC, 0x33 };
    public static byte[] CuReadInputs { get; } = new byte[] { 0x33, 0xCC };
}

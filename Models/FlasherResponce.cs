using System;
using Wrench.DataTypes;
using Wrench.Services;

namespace Wrench.Models;

public class FlasherResponce
{
    public FlasherResponce() {}
    public FlasherResponce(ResponceType type) => ResponceType = type;
    public FlasherResponce(Exception ex)
    {
        ResponceType = ResponceType.Fail;
        ResponceMessage = string.Join(" ", ex.Message, ex.GetType().Name);
    }
    public ResponceType ResponceType { get; set; } = ResponceType.Unsuccess;
    public string? ResponceMessage { get; set; } = string.Empty;
}

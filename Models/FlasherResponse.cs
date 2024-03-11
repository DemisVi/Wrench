using System;
using Wrench.DataTypes;
using Wrench.Services;

namespace Wrench.Models;

public class FlasherResponse
{
    public FlasherResponse() {}
    public FlasherResponse(ResponseType type) => ResponseType = type;
    public FlasherResponse(Exception ex)
    {
        ResponseType = ResponseType.Fail;
        ResponseMessage = string.Join(" ", ex.Message, ex.GetType().Name);
    }
    public ResponseType ResponseType { get; set; } = ResponseType.NotFound;
    public string? ResponseMessage { get; set; } = string.Empty;
}

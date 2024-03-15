using System;
using Wrench.DataTypes;

namespace Wrench.Services;

public class FlasherControllerEventArgs
{
    public FlasherControllerEventArgs(FlasherControllerEventType type) => EventType = type;
    public FlasherControllerEventArgs(FlasherControllerEventType type, object payload) => (EventType, Payload) = (type, payload);

    public FlasherControllerEventType EventType { get; } = FlasherControllerEventType.None;
    public object? Payload { get; set; } = null;
}

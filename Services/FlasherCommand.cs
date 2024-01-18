using System;
using Iot.Device.Bno055;
using Wrench.DataTypes;
using Wrench.Models;

namespace Wrench.Services;

public class FlasherCommand
{
    private Func<FlasherResponse> command;

    public FlasherCommand(Func<FlasherResponse> command) => this.command = command;

    public static FlasherCommand Create(Action action, string commandDescription = "Not described command")
    {
        return new FlasherCommand(new Func<FlasherResponse>(() =>
        {
            try
            {
                action.Invoke();
                return new FlasherResponse(ResponseType.OK);
            }
            catch (Exception ex)
            {
                return new FlasherResponse(ex);
            }
        }))
        { CommandNote = commandDescription, };
    }

    public static FlasherCommand Create<T>(Action<T?> action, T? param, string commandDescription = "Not described command")
    {
        return new FlasherCommand(new Func<FlasherResponse>(() =>
        {
            try
            {
                action.Invoke(param);
                return new FlasherResponse(ResponseType.OK);
            }
            catch (Exception ex)
            {
                return new FlasherResponse(ex);
            }
        }))
        { CommandNote = commandDescription, };
    }

    public static FlasherCommand Create<T, TRes>(Func<T, TRes> func, T param, string commandDescription = "Not described command")
    {
        return new FlasherCommand(new Func<FlasherResponse>(() =>
        {
            try
            {
                return new FlasherResponse(ResponseType.OK) { ResponseMessage = func.Invoke(param)?.ToString() };
            }
            catch (Exception ex)
            {
                return new FlasherResponse(ex);
            }
        }))
        { CommandNote = commandDescription, };
    }

    public string CommandNote { get; set; } = string.Empty;

    public FlasherResponse Execute() => command.Invoke();
}
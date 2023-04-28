using System;
using System.Threading;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Linq;

namespace Wrench.Model;

public class ATWriter : IDisposable
{
    private readonly static object _portAccessLock = new();
    private bool disposedValue;

    public List<ATCommand>? Commands { get; set; }
    public SerialPort Port { get; init; }

    public ATWriter(IModemPortConfig portConfig, string commandsFilePath)
    {
        var lines = File.ReadAllLines(commandsFilePath);
        this.Commands = lines.Select(x => new ATCommand() { Command = x.Split(' ').First(), Answer = x.Split(' ').Last() }).ToList();

        Port = InitPort(portConfig);
    }

    public ATWriter(IModemPortConfig portConfig, List<ATCommand>? commands = null)
    {
        this.Commands = commands;

        this.Port = InitPort(portConfig);
    }

    public ATWriter(SerialPort port, List<ATCommand>? commands = null)
    {
        this.Commands = commands;

        this.Port = port;
    }

    public bool SendCommands() => SendCommands(this.Commands);

    public bool SendCommands(List<ATCommand>? commands)
    {
        if (commands is null || commands is { Count: <= 0 })
            throw new ArgumentNullException(nameof(commands), $"'{nameof(SendCommands)}' parameter can't be null or empty");

        foreach (var i in commands!)
        {
            if (!SendCommand(i))
                return false;
        }
        return true;
    }
    public bool SendCommand(ATCommand command) => SendCommand(this.Port, command);

    public static bool SendCommand(SerialPort port, ATCommand command) => InnerSend(port, command);

    protected static bool InnerSend(SerialPort port, ATCommand command)
    {
        lock (_portAccessLock)
        {
            if (port is { IsOpen: true })
                throw new InvalidOperationException("Port is open elsewere");

            try
            {
                port.Open();
                port.DiscardInBuffer();
                port.DiscardOutBuffer();
                port.WriteLine(command.Command);

                Thread.Sleep(250);

                var res = string.Empty;
                while (port.BytesToRead > 0)
                    res += port.ReadExisting();

                return res.Contains(command.Answer, StringComparison.OrdinalIgnoreCase);
            }
            catch (IOException)
            {
                return false;
            }
            finally
            {
                port.Close();
            }
        }
    }

    protected SerialPort InitPort(IModemPortConfig portConfig) => new SerialPort()
    {
        NewLine = portConfig.NewLine,
        PortName = portConfig.PortName,
        BaudRate = portConfig.BaudRate,
        Handshake = portConfig.HandShake,
        ReadTimeout = portConfig.ReadTimeout,
        WriteTimeout = portConfig.WriteTimeout,
    };

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                Port.Close();
            }

            // TODO: free unmanaged resources (unmanaged objects) and override finalizer
            // TODO: set large fields to null
            disposedValue = true;
        }
    }

    // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
    // ~ATWriter()
    // {
    //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
    //     Dispose(disposing: false);
    // }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}

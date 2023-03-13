using System;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Collections.Generic;

namespace Wrench.Model;

public class ATWriter
{
    private List<ATCommand> commands;
    private IModemPortConfig portConfig;
    public ATWriter(IModemPortConfig portConfig, string commandsFilePath)
    {
        var lines = File.ReadAllLines(commandsFilePath);
        commands = lines.Select(x => new ATCommand() { Command = x.Split(' ').First(), Answer = x.Split(' ').Last() }).ToList();
        this.portConfig = portConfig;
    }

    public bool SendCommands()
    {
        using var port = new SerialPort()
        {
            NewLine = portConfig.NewLine,
            PortName = portConfig.PortName,
            BaudRate = portConfig.BaudRate,
            Handshake = portConfig.HandShake,
            ReadTimeout = portConfig.ReadTimeout,
            WriteTimeout = portConfig.WriteTimeout,
        };
        try
        {
            port.Open();
            port.WriteLine("ate0");
            port.ReadLine();
            port.DiscardInBuffer();

            foreach (var i in commands)
            {
                port.WriteLine(i.Command);
                port.ReadLine();
                if (!port.ReadExisting().Contains(i.Answer))
                    return false;
            }
            return true;
        }
        catch (Exception)
        {
            throw;
        }
        finally
        {
            port.Close();
        }

    }
    public static bool SendCommand(IModemPortConfig portConfig, ATCommand command)
    {
        using var port = new SerialPort()
        {
            NewLine = portConfig.NewLine,
            PortName = portConfig.PortName,
            BaudRate = portConfig.BaudRate,
            Handshake = portConfig.HandShake,
            ReadTimeout = portConfig.ReadTimeout,
            WriteTimeout = portConfig.WriteTimeout,
        };
        try
        {
            port.Open();
            port.WriteLine("ate0");
            port.ReadLine();
            port.DiscardInBuffer();

            port.WriteLine(command.Command);
            port.ReadLine();
            if (!port.ReadExisting().Contains(command.Answer))
            {
                return false;
            }
            else
            {
                return true;
            }
        }
        catch (Exception)
        {
            throw;
        }
        finally
        {
            port.Close();
        }
    }

}
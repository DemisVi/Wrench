using System;
using System.Text;
using System.Linq;
using System.IO.Ports;
using System.Management;
using System.Device.Gpio;
using Iot.Device.Ft2232H;
using Iot.Device.FtCommon;
using System.Threading;

public class ContactUnit : IContactUnit, IDisposable
{
    private bool _disposedValue;

    private GpioOutputs _outputs;
    private GpioInputs _inputs;
    private Ftx232HDevice _cuFtDevice;
    private FtSerialPort _cuSerialPort;
    private GpioPin _powerPin;
    private GpioPin _cl15Pin;

    public GpioOutputs Outputs
    {
        get => _outputs;
        private set
        {
            _outputs = value;
            WriteGpio();
        }
    }
    public GpioInputs Inputs
    {
        get
        {
            ReadGpio();
            return _inputs;
        }
        private set => _inputs = value;
    }

    public ContactUnit(FtSerialPort cuSerialPort, Ftx232HDevice ftDevice)
    {
        _cuSerialPort = cuSerialPort;
        _cuFtDevice = ftDevice;
        using var gpio = _cuFtDevice.CreateGpioController();
        _powerPin = gpio.OpenPin(Ft2232HDevice.GetPinNumberFromString("ADBUS0"), PinMode.Output);
        _cl15Pin = gpio.OpenPin(Ft2232HDevice.GetPinNumberFromString("ADBUS2"), PinMode.Output);
        _cuSerialPort.Open();
    }

    public void LockBoard() => Outputs |= GpioOutputs.Pn1;

    public void ReleaseBoard() => Outputs &= ~GpioOutputs.Pn1;

    public void PowerOffBoard() => _powerPin.Write(PinValue.High);

    public void PowerOnBoard() => _powerPin.Write(PinValue.Low);
    
    public void Cl15Off() => _cl15Pin.Write(PinValue.High);
    
    public void Cl15On() => _cl15Pin.Write(PinValue.Low);

    public void LEDOff() => Outputs &= ~GpioOutputs.White;

    public void LEDBlue()
    {
        LEDOff();
        Outputs |= GpioOutputs.Blue;
    }

    public void LEDGreen()
    {
        LEDOff();
        Outputs |= GpioOutputs.Green;
    }

    public void LEDRed()
    {
        LEDOff();
        Outputs |= GpioOutputs.Red;
    }

    public void LEDYellow()
    {
        LEDOff();
        Outputs |= GpioOutputs.Yellow;
    }

    public void LEDCyan()
    {
        LEDOff();
        Outputs |= GpioOutputs.Cyan;
    }

    public void LEDMagenta()
    {
        LEDOff();
        Outputs |= GpioOutputs.Magenta;
    }

    public void LEDWhite()
    {
        LEDOff();
        Outputs |= GpioOutputs.White;
    }

    private void WriteGpio()
    {
        _cuSerialPort.Write(CuCommands.CuWriteOutputs, 0, CuCommands.CuWriteOutputs.Length);
        _cuSerialPort.Write(new byte[] { (byte)Outputs }, 0, 1);
    }

    private void ReadGpio()
    {
        _cuSerialPort.Write(CuCommands.CuReadInputs, 0, CuCommands.CuReadInputs.Length);
        Thread.Sleep(20);
        var received = new byte[3];
        _cuSerialPort.Read(received, 0, received.Length);
        Inputs = (GpioInputs)received[2];
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                // TODO: dispose managed state (managed objects)
                _cuSerialPort.Dispose();
            }

            // TODO: free unmanaged resources (unmanaged objects) and override finalizer
            // TODO: set large fields to null
            _disposedValue = true;
        }
    }

    // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
    // ~ContactUnit()
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

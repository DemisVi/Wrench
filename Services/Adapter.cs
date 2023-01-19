using System;
using System.Timers;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Win32;
using FTD2XX_NET;

namespace Wrench.Services;

/// <summary>
/// Класс Adapter интерфейсные функции 
/// ----------------------------------
/// Класс Adapter является оболочкой к FDI адаптеру USB-CAN (отдельно для портов 'А' и 'В')
/// Интерфейсные функции: 
///     delegate void logFunction(string msg);   // фидбэк для логов
///     Adapter(logFunction log)                // конструктор класса
///     public FTDI.FT_STATUS OpenAdapter(string serial)    // серийный номер без суффикса 'A' или 'B'
///     public void CloseAdapter()
///     public void Disconnect()                // close connection
///     public bool ResetCAN()                  // только для порта 'В'
///     public bool SetDataCharacteristics(byte DataBits, byte StopBits, byte Parity)
///     public bool SetFlowControl(UInt16 FlowControl, byte Xon, byte Xoff)
///     public bool SetBaudRate(UInt32 BaudRate)
///     public bool SetTimeouts(UInt32 ReadTimeout, UInt32 WriteTimeout)
///     public bool PurgeRx()
///     public bool PurgeUART()
///     public bool GetSensorState()
///     public void KL15_On()                   // управление Кл15
///     public void KL15_Off()
///     public void KL30_On()                   // управление Кл30
///     public void KL30_Off()
///     public void GreenOn()                   // функции DIO для светодиодов
///     public void GreenOff()
///     public void YellowOn()
///     public void YellowOff()
///     public void RedOn()
///     public void RedOff()
///     public void LEDsOff() 
/// </summary>
//-------------------------------------------------------------------------

public class Adapter : IDisposable
{
    [Flags]
    enum PortBits
    {
        KL30 = 1,
        Sensor = 2,
        KL15 = 4,
        Yellow = 8,
        Green = 16,
        Pgm = 32,
        Red = 64
    }


    protected FTDI.FT_STATUS _ftStatus = FTDI.FT_STATUS.FT_OTHER_ERROR;
    protected FTDI _myFtdiDevice = new();
    protected bool _disposed = false;
    protected byte _fBits = 0;
    protected bool _oldSensorState;
    protected Action<string>? _logger;
    protected System.Timers.Timer _sensorTimer;
    public string SerialNum { get; private set; }
    public bool IsOpen => _myFtdiDevice.IsOpen;

    // Constructor 
    public Adapter(string adapterSerial, Action<string>? log = null)
    {
        _logger = log;
        _sensorTimer = new()
        {
            Interval = 1_000D,
            Enabled = false,
            AutoReset = true,
        };
        SerialNum = adapterSerial;
        _sensorTimer.Elapsed += OnSensorStateChanged;
        _oldSensorState = SetInitialSensorState();
    }

    private bool SetInitialSensorState()
    {
        OpenAdapter();
        var state = GetSensorState();
        CloseAdapter();
        return state;
    }

    public event AdapterSensorEventHandler? SensorStateChanged;

    public virtual bool OpenAdapter()
    {
        var status = _myFtdiDevice.OpenBySerialNumber(SerialNum);

        if (status != FTDI.FT_STATUS.FT_OK)
        {
            Log("Ошибка подключения адаптера.");
            return false;
        }
        else Log("is connected.");

        if (SerialNum.EndsWith('A'))
        {
            // обнулть все выходы
            _fBits = 0;
            UpdateBitsInvert();
        }
        return status == FTDI.FT_STATUS.FT_OK;
    } // void OpenAdapter(string serial)
    //----------------------------------------------------

    public bool CloseAdapter()
    {
        return FTDI.FT_STATUS.FT_OK == _myFtdiDevice.Close();
    } // void CloseAdapter()
    //----------------------------------------------------


    protected void Log(String message) => _logger?.Invoke(SerialNum + ": " + message);

    // close connection
    public void Disconnect()
    {
        _fBits = 0;
        // обнулть все выходы
        UpdateBitsInvert();
        _myFtdiDevice.Close();
    } // void Disconnect()
    //---------------------------------------


    public bool ResetCAN()
    {
        if (SerialNum!.EndsWith('B'))
        {
            Log("ResetCAN() недопустим для порта " + SerialNum);
            return false;
        }
        _ftStatus = _myFtdiDevice.SetRTS(true);
        if (_ftStatus == FTDI.FT_STATUS.FT_OK)
        {
            Task.Delay(50).Wait();
            _ftStatus = _myFtdiDevice.SetRTS(false);
        }
        return (_ftStatus == FTDI.FT_STATUS.FT_OK);
    } // void ResetCAN()
    //---------------------------------------


    public bool SetBaudRate(UInt32 BaudRate)
    {
        _ftStatus = _myFtdiDevice.SetBaudRate(BaudRate);
        return (_ftStatus == FTDI.FT_STATUS.FT_OK);
    } // SetBaudRate
      //---------------------------------------


    public bool SetDataCharacteristics(byte DataBits, byte StopBits, byte Parity)
    {
        _ftStatus = _myFtdiDevice.SetDataCharacteristics(DataBits, StopBits, Parity);
        return (_ftStatus == FTDI.FT_STATUS.FT_OK);
    } // SetDataCharacteristics
    //--------------------------------------


    public bool SetFlowControl(UInt16 FlowControl, byte Xon, byte Xoff)
    {
        _ftStatus = _myFtdiDevice.SetFlowControl(FlowControl, Xon, Xoff);
        return (_ftStatus == FTDI.FT_STATUS.FT_OK);
    } // SetFlowControl
    //----------------------------------------------------------


    public bool SetTimeouts(UInt32 ReadTimeout, UInt32 WriteTimeout)
    {
        _ftStatus = _myFtdiDevice.SetTimeouts(ReadTimeout, WriteTimeout);
        return (_ftStatus == FTDI.FT_STATUS.FT_OK);
    } // SetTimeouts
      //-----------------------------------------------------------


    public bool PurgeRx()
    {
        _ftStatus = _myFtdiDevice.Purge(FTDI.FT_PURGE.FT_PURGE_RX);
        return (_ftStatus == FTDI.FT_STATUS.FT_OK);
    } // bool Purge
    //----------------------------------------


    public bool PurgeUART()
    {
        _ftStatus = _myFtdiDevice.Purge(FTDI.FT_PURGE.FT_PURGE_RX + FTDI.FT_PURGE.FT_PURGE_TX);
        return (_ftStatus == FTDI.FT_STATUS.FT_OK);
    } // bool Purge
    //----------------------------------------


    // датчик контактного устройства
    public bool GetSensorState()
    {
        Byte bits = 0;
        _ftStatus = _myFtdiDevice.GetPinStates(ref bits);
        if (_ftStatus == FTDI.FT_STATUS.FT_OK)
            return (bits & (byte)PortBits.Sensor) == 0;
        else return false;
    } // bool GetSensorState()
    //---------------------------------------


    // инвертирует (из-за опторазвязки) и записывает биты набора FBits в битовый порт
    protected void UpdateBitsInvert()
    {
        UInt32 nb = 0;
        byte[] buff = new byte[3];
        _ftStatus = _myFtdiDevice.SetBitMode(0x00, FTDI.FT_BIT_MODES.FT_BIT_MODE_MPSSE);
        if (_ftStatus == FTDI.FT_STATUS.FT_OK)
        {
            // buff[1] - data, buff[2] - DIR
            buff[0] = 0x80;   // ID ADBUS
            buff[1] = (byte)~_fBits;   //  data values
            buff[2] = 0xFD;  // bit 1 - sensor input, other - outputs
            _ftStatus = _myFtdiDevice.Write(buff, 3, ref nb);
        }
        else Log("Ошибка инициализаци битового порта");
    } // void UpdateBitsInvert()
    //---------------------------------------


    // Power control
    public bool KL15_On()
    {
        _fBits |= (byte)PortBits.KL15;
        UpdateBitsInvert();
        if (_ftStatus != FTDI.FT_STATUS.FT_OK)
        {
            Log("Ошибка включения питания устройства.");
            return false;
        }
        else return true;
    } // void KL15_On()
    //---------------------------------------

    // Power control
    public bool KL15_Off()
    {
        int mask = (int)PortBits.KL15;
        _fBits &= (byte)~mask;
        UpdateBitsInvert();
        if (_ftStatus != FTDI.FT_STATUS.FT_OK)
        {
            Log("Ошибка выключения питания устройства.");
            return false;
        }
        else return true;
    } // KL15_Off()
    //---------------------------------------

    // Power control
    public bool KL30_On()
    {
        _fBits |= (byte)PortBits.KL30;
        UpdateBitsInvert();
        if (_ftStatus != FTDI.FT_STATUS.FT_OK)
        {
            Log("Ошибка включения питания устройства.");
            return false;
        }
        else return true;
    } // void KL30_On()
    //---------------------------------------

    // Power control
    public bool KL30_Off()
    {
        int mask = (int)PortBits.KL30;
        _fBits &= (byte)~mask;
        UpdateBitsInvert();
        if (_ftStatus != FTDI.FT_STATUS.FT_OK)
        {
            Log("Ошибка выключения питания устройства.");
            return false;
        }
        else return true;
    } // KL30_Off()
    //---------------------------------------


    //
    // LED control
    //
    public void GreenOn()
    {
        _fBits |= (byte)PortBits.Green;
        UpdateBitsInvert();
        if (_ftStatus != FTDI.FT_STATUS.FT_OK)
            Log("Ошибка включения зеленого светофора");
    } // void GreenOn()
      //---------------------------------------


    public void GreenOff()
    {
        uint mask = ~(uint)PortBits.Green;
        _fBits &= (byte)mask;
        UpdateBitsInvert();
        if (_ftStatus != FTDI.FT_STATUS.FT_OK)
            Log("Ошибка выключения зеленого светофора");
    } // void GreenOff()
    //---------------------------------------


    public void YellowOn()
    {
        _fBits |= (byte)PortBits.Yellow;
        UpdateBitsInvert();
        if (_ftStatus != FTDI.FT_STATUS.FT_OK)
            Log("Ошибка включения желтого светофора");
    }
    //---------------------------------------


    public void YellowOff()
    {
        uint mask = ~(uint)PortBits.Yellow;
        _fBits &= (byte)mask;
        UpdateBitsInvert();
        if (_ftStatus != FTDI.FT_STATUS.FT_OK)
            Log("Ошибка выключения желтого светофора");
    } // void YellowOn()
    //---------------------------------------


    public void RedOn()
    {
        _fBits |= (byte)PortBits.Red;
        UpdateBitsInvert();
        if (_ftStatus != FTDI.FT_STATUS.FT_OK)
            Log("Ошибка включения красного светофора");
    } // void RedOn()
    //---------------------------------------


    public void RedOff()
    {
        uint mask = (uint)PortBits.Red;
        _fBits &= (byte)~mask;
        UpdateBitsInvert();
        if (_ftStatus != FTDI.FT_STATUS.FT_OK)
            Log("Ошибка выключения крастного светофора");
    } // void RedOff()
    //---------------------------------------


    public void LEDsOff()
    {
        uint mask = ((Byte)PortBits.Green + (Byte)PortBits.Red + (Byte)PortBits.Yellow);
        _fBits &= (byte)~mask;
        UpdateBitsInvert();
        if (_ftStatus != FTDI.FT_STATUS.FT_OK)
            Log("Ошибка выключения светофора");
    }

    public void BeginWatchSensorState()
    {
        if (!IsOpen) throw new InvalidOperationException("Adapter port closed and cannot report sensor state");
        _oldSensorState = GetSensorState();
        _sensorTimer?.Start();
    }

    public void EndWatchSensorState() => _sensorTimer?.Stop();

    private void OnSensorStateChanged(object? sender, ElapsedEventArgs e)
    {
        var state = GetSensorState();
        if (state != _oldSensorState)
        {
            _oldSensorState = state;
            SensorStateChanged?.Invoke(this, new AdapterSensorEventArgs(state));
        }
    }

    internal void WaitForSensor(bool state)
    {
        if (!IsOpen) throw new InvalidOperationException("Adapter is closed and can't report sensor state");
        while (state != GetSensorState())
            Thread.Sleep(100);
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing)
    {
        if (_disposed) return;

        _myFtdiDevice.Close();

        _disposed = true;
    }

    ~Adapter()
    {
        Dispose(false);
    }
}

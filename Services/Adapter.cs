using System;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FTD2XX_NET;

// Версия 2: доработана настройка порта для UART
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
// делегат для сообщений об ошибках

// Клас-оболочка адаптера программирования БЭГ (использованы функции портов "A" и "B")
public class Adapter : IDisposable
{
    public delegate void LogFunction(string msg);

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

    //
    // Members
    //

    protected FTDI.FT_STATUS ftStatus = FTDI.FT_STATUS.FT_OTHER_ERROR;
    protected FTDI myFtdiDevice = new();
    protected bool disposed = false;
    protected LogFunction? _logger;
    protected byte FBits = 0;
    public string? SerialNum { get; private set; } = "";
    public bool IsOpen => myFtdiDevice.IsOpen;

    // Constructor 
    public Adapter(LogFunction? log = null) => _logger = log;

    //
    // Methods --------------------------
    //

    public virtual bool OpenAdapter(string? serial)
    {
        if (serial == null) throw new("Adapter serial can not be null");
        SerialNum = serial;
        var status = FTDI.FT_STATUS.FT_OTHER_ERROR;

        status = myFtdiDevice.OpenBySerialNumber(SerialNum);

        if (status != FTDI.FT_STATUS.FT_OK)
        {
            Log("Ошибка подключения адаптера.");
            return false;
        }
        else Log("is connected.");

        if (serial.EndsWith('A'))
        {
            // обнулть все выходы
            FBits = 0;
            UpdateBitsInvert();
        }
        return status == FTDI.FT_STATUS.FT_OK;
    } // void OpenAdapter(string serial)
    //----------------------------------------------------

    public void CloseAdapter()
    {
        myFtdiDevice.Close();
    } // void CloseAdapter()
    //----------------------------------------------------


    protected void Log(String message) => _logger?.Invoke(SerialNum + ": " + message);

    // close connection
    public void Disconnect()
    {
        FBits = 0;
        // обнулть все выходы
        UpdateBitsInvert();
        myFtdiDevice.Close();
    } // void Disconnect()
    //---------------------------------------


    public bool ResetCAN()
    {
        if (SerialNum.EndsWith('B'))
        {
            Log("ResetCAN() недопустим для порта " + SerialNum);
            return false;
        }
        ftStatus = myFtdiDevice.SetRTS(true);
        if (ftStatus == FTDI.FT_STATUS.FT_OK)
        {
            Thread.Sleep(50);
            ftStatus = myFtdiDevice.SetRTS(false);
        }
        return (ftStatus == FTDI.FT_STATUS.FT_OK);
    } // void ResetCAN()
    //---------------------------------------


    public bool SetBaudRate(UInt32 BaudRate)
    {
        ftStatus = myFtdiDevice.SetBaudRate(BaudRate);
        return (ftStatus == FTDI.FT_STATUS.FT_OK);
    } // SetBaudRate
      //---------------------------------------


    public bool SetDataCharacteristics(byte DataBits, byte StopBits, byte Parity)
    {
        ftStatus = myFtdiDevice.SetDataCharacteristics(DataBits, StopBits, Parity);
        return (ftStatus == FTDI.FT_STATUS.FT_OK);
    } // SetDataCharacteristics
    //--------------------------------------


    public bool SetFlowControl(UInt16 FlowControl, byte Xon, byte Xoff)
    {
        ftStatus = myFtdiDevice.SetFlowControl(FlowControl, Xon, Xoff);
        return (ftStatus == FTDI.FT_STATUS.FT_OK);
    } // SetFlowControl
    //----------------------------------------------------------


    public bool SetTimeouts(UInt32 ReadTimeout, UInt32 WriteTimeout)
    {
        ftStatus = myFtdiDevice.SetTimeouts(ReadTimeout, WriteTimeout);
        return (ftStatus == FTDI.FT_STATUS.FT_OK);
    } // SetTimeouts
      //-----------------------------------------------------------


    public bool PurgeRx()
    {
        ftStatus = myFtdiDevice.Purge(FTDI.FT_PURGE.FT_PURGE_RX);
        return (ftStatus == FTDI.FT_STATUS.FT_OK);
    } // bool Purge
    //----------------------------------------


    public bool PurgeUART()
    {
        ftStatus = myFtdiDevice.Purge(FTDI.FT_PURGE.FT_PURGE_RX + FTDI.FT_PURGE.FT_PURGE_TX);
        return (ftStatus == FTDI.FT_STATUS.FT_OK);
    } // bool Purge
    //----------------------------------------


    // датчик контактного устройства
    public bool GetSensorState()
    {
        Byte bits = 0;
        ftStatus = myFtdiDevice.GetPinStates(ref bits);
        if (ftStatus == FTDI.FT_STATUS.FT_OK)
            return (bits & (byte)PortBits.Sensor) == 0;
        else return false;
    } // bool GetSensorState()
    //---------------------------------------


    // инвертирует (из-за опторазвязки) и записывает биты набора FBits в битовый порт
    protected void UpdateBitsInvert()
    {
        UInt32 nb = 0;
        byte[] buff = new byte[3];
        ftStatus = myFtdiDevice.SetBitMode(0x00, FTDI.FT_BIT_MODES.FT_BIT_MODE_MPSSE);
        if (ftStatus == FTDI.FT_STATUS.FT_OK)
        {
            // buff[1] - data, buff[2] - DIR
            buff[0] = 0x80;   // ID ADBUS
            buff[1] = (byte)~FBits;   //  data values
            buff[2] = 0xFD;  // bit 1 - sensor input, other - outputs
            ftStatus = myFtdiDevice.Write(buff, 3, ref nb);
        }
        else Log("Ошибка инициализаци битового порта");
    } // void UpdateBitsInvert()
    //---------------------------------------


    // Power control
    public void KL15_On()
    {
        FBits |= (byte)PortBits.KL15;
        UpdateBitsInvert();
        if (ftStatus != FTDI.FT_STATUS.FT_OK)
            Log("Ошибка включения питания устройства.");
    } // void KL15_On()
    //---------------------------------------

    // Power control
    public void KL15_Off()
    {
        int mask = (int)PortBits.KL15;
        FBits &= (byte)~mask;
        UpdateBitsInvert();
        if (ftStatus != FTDI.FT_STATUS.FT_OK)
            Log("Ошибка выключения питания устройства.");
    } // KL15_Off()
    //---------------------------------------

    // Power control
    public void KL30_On()
    {
        FBits |= (byte)PortBits.KL30;
        UpdateBitsInvert();
        if (ftStatus != FTDI.FT_STATUS.FT_OK)
            Log("Ошибка включения питания устройства.");
    } // void KL30_On()
    //---------------------------------------

    // Power control
    public void KL30_Off()
    {
        int mask = (int)PortBits.KL30;
        FBits &= (byte)~mask;
        UpdateBitsInvert();
        if (ftStatus != FTDI.FT_STATUS.FT_OK)
            Log("Ошибка выключения питания устройства.");
    } // KL30_Off()
    //---------------------------------------


    //
    // LED control
    //
    public void GreenOn()
    {
        FBits |= (byte)PortBits.Green;
        UpdateBitsInvert();
        if (ftStatus != FTDI.FT_STATUS.FT_OK)
            Log("Ошибка включения зеленого светофора");
    } // void GreenOn()
      //---------------------------------------


    public void GreenOff()
    {
        uint mask = ~(uint)PortBits.Green;
        FBits &= (byte)mask;
        UpdateBitsInvert();
        if (ftStatus != FTDI.FT_STATUS.FT_OK)
            Log("Ошибка выключения зеленого светофора");
    } // void GreenOff()
    //---------------------------------------


    public void YellowOn()
    {
        FBits |= (byte)PortBits.Yellow;
        UpdateBitsInvert();
        if (ftStatus != FTDI.FT_STATUS.FT_OK)
            Log("Ошибка включения желтого светофора");
    }
    //---------------------------------------


    public void YellowOff()
    {
        uint mask = ~(uint)PortBits.Yellow;
        FBits &= (byte)mask;
        UpdateBitsInvert();
        if (ftStatus != FTDI.FT_STATUS.FT_OK)
            Log("Ошибка выключения желтого светофора");
    } // void YellowOn()
    //---------------------------------------


    public void RedOn()
    {
        FBits |= (byte)PortBits.Red;
        UpdateBitsInvert();
        if (ftStatus != FTDI.FT_STATUS.FT_OK)
            Log("Ошибка включения красного светофора");
    } // void RedOn()
    //---------------------------------------


    public void RedOff()
    {
        uint mask = (uint)PortBits.Red;
        FBits &= (byte)~mask;
        UpdateBitsInvert();
        if (ftStatus != FTDI.FT_STATUS.FT_OK)
            Log("Ошибка выключения крастного светофора");
    } // void RedOff()
    //---------------------------------------


    public void LEDsOff()
    {
        uint mask = ((Byte)PortBits.Green + (Byte)PortBits.Red + (Byte)PortBits.Yellow);
        FBits &= (byte)~mask;
        UpdateBitsInvert();
        if (ftStatus != FTDI.FT_STATUS.FT_OK)
            Log("Ошибка выключения светофора");
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing)
    {
        if (disposed) return;

        myFtdiDevice.Close();

        disposed = true;
    }
    //---------------------------------------

    ~Adapter()
    {
        Dispose(false);
    } // ~Adapter()
      //--------------------------------------------------------
}

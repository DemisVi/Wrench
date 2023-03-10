using Wrench.Model;

namespace Wrench.Interfaces
{
    public interface IContactUnit
    {
        bool PowerOff();
        bool PowerOn();
        Outs SetOuts(Outs outs);
        Sensors WaitForBits(Sensors sensors, int timeout = -1);
        Sensors WaitForState(Sensors sensors, int timeout = -1);
    }
}

namespace Wrench.DataTypes;

public class Constants
{
#if OOO
    public const string DepartmentName = "ООО";
    public const GpioInputs deviceCUReadyState = GpioInputs.Lodg;
#else
    public const string DepartmentName = "ОПП";
    public const GpioInputs deviceCUReadyState = GpioInputs.Lodg | GpioInputs.Device | GpioInputs.Pn1_Down;
#endif
}

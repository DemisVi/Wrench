public interface IContactUnit
{
    public GpioOutputs Outputs { get; }
    public GpioInputs Inputs { get; }
    public void LockBoard();
    public void ReleaseBoard();
    public void PowerOffBoard();
    public void PowerOnBoard();
    public void Cl15Off();
    public void Cl15On();
    public void LEDOff();
    public void LEDBlue();
    public void LEDGreen();
    public void LEDRed();
    public void LEDYellow();
    public void LEDCyan();
    public void LEDMagenta();
    public void LEDWhite();
}

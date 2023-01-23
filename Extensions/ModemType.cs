namespace Wrench.Extensions;

public abstract class ModemType
{
    public virtual string BootCommand { get; } = "AT";
}

public class SimComModem : ModemType
{
    public override string BootCommand { get; } = "AT+CGSN";
}

public class SimComADB : ModemType
{
    public override string BootCommand { get; } = "AT+CUSBADB=1";
}

public class TelitModem : ModemType
{
    public override string BootCommand { get; } = "AT+GSN=1";
}
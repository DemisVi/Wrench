namespace Wrench.Extensions;

public abstract class ModemType
{
    public virtual string Type { get; } = "AT";
}

public class SimComModem : ModemType
{
    public override string Type { get; } = "AT+CGSN";
}

public class TelitModem : ModemType
{
    public override string Type { get; } = "AT+GSN=1";
}
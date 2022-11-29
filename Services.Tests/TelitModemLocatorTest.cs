namespace Wrench.Services.Tests;

public class TelitModemLocatorTest
{
    ModemLocator loc = new(LocatorQuery.queryEventTelit, LocatorQuery.queryTelitModem);
    object syncLock = new();

    [Fact]
    public void ShouldReactDeviceConnected()
    {
        var obj = loc.WaitDeviceConnect();
        Assert.NotNull(obj);
    }

    [Fact]
    public void ShouldReturnFewSerials()
    {
            Assert.NotEmpty(loc.LocateDevices());
    }

    [Fact]
    public void ShouldReturnAsyncFewSerials()
    {
            Assert.NotEmpty(loc.LocateDevicesAsync().GetAwaiter().GetResult());
    }

    [Fact]
    public void ShouldSerialsFormatBeCorrect()
    {
            Assert.Contains("COM", loc.LocateDevices().Select(x => x.AttachedTo.Substring(0, 3)));
            foreach (var i in loc.LocateDevices().Select(x => x.AttachedTo))
                Assert.True(int.TryParse(i.Substring(3), out var _));
    }

    [Fact]
    public void ShouldSerialsFormatBeCorrect2()
    {
            Assert.Contains("COM", loc.GetModemPortNames().Select(x => x.Substring(0, 3)));
            foreach (var i in loc.GetModemPortNames())
                Assert.True(int.TryParse(i.Substring(3), out var _));
    }
}
namespace Wrench.Services.Tests;

public class AdapterLocatorTest
{
    AdapterLocator loc = new();

    [Fact]
    public void ShouldContainNumberOfAdapters()
    {
        Assert.True(loc.AdapterCount > 0);
        Assert.Equal<uint>(4, loc.AdapterCount);
    }

    [Fact]
    public void ShouldContainFewSerials()
    {
        Assert.NotEmpty(loc.AdapterSerials);
    }

    [Fact]
    public void ShouldContainSerialStrings()
    {
        foreach (var i in loc.AdapterSerials)
            Assert.NotEmpty(i);
    }

    [Fact]
    public void ShouldAdapterCountEqualsSerialsCount()
    {
        Assert.Equal((int)loc.AdapterCount, loc.AdapterSerials.Count);
    }

    [Fact]
    public void ShouldContainKnownNames()
    {
        Assert.Contains("USBCOM17A", loc.AdapterSerials);
        Assert.Contains("USBCOM17B", loc.AdapterSerials);
        //Assert.Contains("USBCOM20A", loc.AdapterSerials);
        //Assert.Contains("USBCOM20B", loc.AdapterSerials);
    }
}

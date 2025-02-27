using ActivityImporter.Engine.Graph;

namespace UnitTests;

[TestClass]
public class O365Tests : AbstractTest
{

    [TestMethod]
    public void OfficeLicenseNameResolverTest()
    {
        var resolver = new OfficeLicenseNameResolver();
        Assert.IsTrue(resolver.GetDisplayNameFor("DYN365_BUSCENTRAL_ESSENTIAL") == "Dynamics 365 Business Central Essentials");

        Assert.IsNull(resolver.GetDisplayNameFor(""));
    }

}

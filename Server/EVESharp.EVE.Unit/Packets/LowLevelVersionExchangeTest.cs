using EVESharp.EVE.Packets;
using EVESharp.Types;
using NUnit.Framework;
using TestExtensions;

namespace EVESharp.EVE.Unit.Packets;

public class LowLevelVersionExchangeTests
{
    public static void AssertLowLevelVersionExchange (PyDataType data, int expectedUserCount = 0)
    {
        (PyInteger birthday, PyInteger machoVersion, PyInteger userCount, PyDecimal version, PyInteger build, PyString codenameRegion) = 
            PyAssert.Tuple <PyInteger, PyInteger, PyInteger, PyDecimal, PyInteger, PyString> (data, false, 6);

        // validate data in it
        PyAssert.Integer (birthday,     Data.Version.BIRTHDAY);
        PyAssert.Integer (build, Data.Version.BUILD);
        PyAssert.String (codenameRegion, Data.Version.CODENAME + "@" + Data.Version.REGION);
        PyAssert.Integer (machoVersion, Data.Version.MACHO_VERSION);
        PyAssert.Decimal (version, Data.Version.VERSION);
        PyAssert.Integer (userCount, expectedUserCount);
    }

    [Test]
    public void LowLevelVersionExchangeBuild ()
    {
        LowLevelVersionExchange ex = new LowLevelVersionExchange
        {
            Birthday = Data.Version.BIRTHDAY,
            Build = Data.Version.BUILD,
            MachoVersion = Data.Version.MACHO_VERSION,
            Version = Data.Version.VERSION,
            UserCount = 0,
            Codename = Data.Version.CODENAME,
            Region = Data.Version.REGION
        };
        
        AssertLowLevelVersionExchange (ex);
    }

    [Test]
    public void LowLevelVersionExchangeParse ()
    {
        PyDataType data = new LowLevelVersionExchange
        {
            Birthday     = Data.Version.BIRTHDAY,
            Build        = Data.Version.BUILD,
            MachoVersion = Data.Version.MACHO_VERSION,
            Version      = Data.Version.VERSION,
            UserCount    = 0,
            Codename     = Data.Version.CODENAME,
            Region       = Data.Version.REGION
        };

        LowLevelVersionExchange ex = data;

        Assert.AreEqual (Data.Version.BIRTHDAY,                             ex.Birthday);
        Assert.AreEqual (Data.Version.BUILD,                                ex.Build);
        Assert.AreEqual (Data.Version.MACHO_VERSION,                        ex.MachoVersion);
        Assert.AreEqual (Data.Version.VERSION,                              ex.Version);
        Assert.AreEqual (Data.Version.CODENAME + "@" + Data.Version.REGION, ex.Codename);
    }
}
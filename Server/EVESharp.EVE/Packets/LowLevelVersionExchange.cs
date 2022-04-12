using EVESharp.Common;
using System;
using System.IO;
using EVESharp.EVE.Client;
using EVESharp.PythonTypes.Types.Collections;
using EVESharp.PythonTypes.Types.Primitives;

namespace EVESharp.EVE.Packets;

public class LowLevelVersionExchange
{
    public int    Birthday     { get; init; }
    public int    MachoVersion { get; init; }
    public int    UserCount    { get; set; }
    public double Version      { get; init; }
    public int    Build        { get; init; }
    public string Codename     { get; init; }
    public string Region       { get; init; }

    public static implicit operator LowLevelVersionExchange(PyDataType data)
    {
        if (data is not PyTuple exchange || exchange.Count != 6)
            throw new InvalidDataException("LowLevelVersionExchange must have 6 elements");

        LowLevelVersionExchange result = new LowLevelVersionExchange
        {
            Birthday     = exchange[0] as PyInteger,
            MachoVersion = exchange[1] as PyInteger,
            Version      = exchange[3] as PyDecimal,
            Build        = exchange[4] as PyInteger,
            Codename     = exchange[5] as PyString,
        };
            
        result.UserCount = exchange[2] as PyInteger;
            
        if (result.Birthday != VersionInfo.BIRTHDAY)
            throw new InvalidDataException("Wrong birthday in LowLevelVersionExchange");
        if (result.Build != VersionInfo.BUILD)
            throw new InvalidDataException("Wrong build in LowLevelVersionExchange");
        if (result.Codename != VersionInfo.CODENAME + "@" + VersionInfo.REGION)
            throw new InvalidDataException("Wrong codename in LowLevelVersionExchange");
        if (result.MachoVersion != VersionInfo.MACHO_VERSION)
            throw new InvalidDataException("Wrong machoVersion in LowLevelVersionExchange");
        if (Math.Abs(result.Version - VersionInfo.VERSION) > 0.001)
            throw new InvalidDataException("Wrong version in LowLevelVersionExchange");

        return result;
    }

    public static implicit operator PyDataType(LowLevelVersionExchange exchange)
    {
        return new PyTuple(6)
        {
            [0] = exchange.Birthday,
            [1] = exchange.MachoVersion,
            [2] = exchange.UserCount,
            [3] = exchange.Version,
            [4] = exchange.Build,
            [5] = exchange.Codename + "@" + exchange.Region
        };
    }
}
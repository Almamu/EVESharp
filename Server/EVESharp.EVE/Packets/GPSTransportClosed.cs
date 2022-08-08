using System;
using EVESharp.PythonTypes.Types.Collections;
using EVESharp.PythonTypes.Types.Network;
using EVESharp.PythonTypes.Types.Primitives;

namespace EVESharp.EVE.Packets;

public class GPSTransportClosed : PyException
{
    private const string TYPE_NAME = "exceptions.GPSTransportClosed";

    public GPSTransportClosed(string type) : base(TYPE_NAME, type, null, new PyDictionary())
    {
        Clock        = DateTime.UtcNow.ToFileTimeUtc();
        Region       = EVE.Data.Version.REGION;
        Reason       = type;
        Version      = EVE.Data.Version.VERSION;
        Build        = EVE.Data.Version.BUILD;
        Codename     = EVE.Data.Version.CODENAME;
        MachoVersion = EVE.Data.Version.MACHO_VERSION;
    }

    public static implicit operator PyDataType(GPSTransportClosed exception)
    {
        exception.Keywords["reasonArgs"]   = exception.ReasonArgs;
        exception.Keywords["clock"]        = exception.Clock;
        exception.Keywords["region"]       = exception.Region;
        exception.Keywords["reason"]       = exception.Reason;
        exception.Keywords["version"]      = exception.Version;
        exception.Keywords["build"]        = exception.Build;
        exception.Keywords["codename"]     = exception.Codename;
        exception.Keywords["machoVersion"] = exception.MachoVersion;

        return exception as PyException;
    }

    public static implicit operator GPSTransportClosed(PyDataType exception)
    {
        PyException ex = exception;

        if (ex.Type != TYPE_NAME)
            throw new Exception($"Expected type {TYPE_NAME} but got {ex.Type}");

        GPSTransportClosed result = new GPSTransportClosed(ex.Reason)
        {
            Keywords     = ex.Keywords,
            ReasonArgs   = ex.Keywords["reasonArgs"] as PyDictionary,
            Clock        = ex.Keywords["clock"] as PyInteger,
            Region       = ex.Keywords["region"] as PyString,
            Reason       = ex.Keywords["reason"] as PyString,
            Version      = ex.Keywords["version"] as PyDecimal,
            Build        = ex.Keywords["build"] as PyInteger,
            Codename     = ex.Keywords["codename"] as PyString,
            MachoVersion = ex.Keywords["machoVersion"] as PyInteger
        };

        return result;
    }

    public string       ExceptionType { get; init; }
    public string       message = "";
    public string       Origin            { get; init; }
    public PyDictionary ReasonArgs        { get; init; } = new PyDictionary();
    public long         Clock             { get; init; }
    public PyDataType   LoggedOnUserCount { get; init; }
    public string       Region            { get; init; }
    public double       Version           { get; init; }
    public int          Build             { get; init; }
    public string       ReasonCode        { get; init; }
    public string       Codename          { get; init; }
    public int          MachoVersion      { get; init; }
}
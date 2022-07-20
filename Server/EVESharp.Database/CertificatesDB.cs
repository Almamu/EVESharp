using System;
using System.Collections.Generic;
using EVESharp.PythonTypes.Types.Database;

namespace EVESharp.Database;

public static class CertificatesDB
{
    public static Rowset CrtGetCharacterCertificates (this IDatabaseConnection Database, int characterID)
    {
        return Database.Rowset ("CrtGetCharacterCertificates", new Dictionary <string, object> () {{"_characterID", characterID}});
    }

    public static void CrtUpdateVisibilityFlags (this IDatabaseConnection Database, int characterID, int certificateID, int flags)
    {
        Database.Query (
            "CrtUpdateVisibilityFlags",
            new Dictionary <string, object> ()
            {
                {"_characterID", characterID},
                {"_certificateID", certificateID},
                {"_flags", flags}
            }
        );
    }

    public static void CrtGrantCertificate (this IDatabaseConnection Database, int characterID, int certificateID)
    {
        Database.Query (
            "CrtGrantCertificate",
            new Dictionary<string, object> ()
            {
                {"_characterID", characterID},
                {"_certificateID", certificateID},
                {"_grantDate", DateTime.Now.ToFileTimeUtc()}
            }
        );
    }
}
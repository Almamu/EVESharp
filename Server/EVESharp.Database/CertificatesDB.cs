using System.Collections.Generic;
using EVESharp.PythonTypes.Types.Database;

namespace EVESharp.Database;

public static class CertificatesDB
{
    public static Rowset CrtGetMyCertificates (this IDatabaseConnection Database, int characterID)
    {
        return Database.Rowset ("CrtGetMyCertificates", new Dictionary <string, object> () {{"_characterID", characterID}});
    }
}
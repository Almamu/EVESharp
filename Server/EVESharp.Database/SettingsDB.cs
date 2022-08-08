using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using EVESharp.EVE.Data.Configuration;
using EVESharp.PythonTypes.Types.Database;

namespace EVESharp.Database;

public static class SettingsDB
{
    public static Dictionary <string, Constant> EveLoadConstants (this IDatabaseConnection Database)
    {
        IDbConnection   connection = null;
        DbDataReader reader     = Database.Select (ref connection, "SELECT constantID, constantValue FROM eveConstants");

        using (connection)
        using (reader)
        {
            Dictionary <string, Constant> result = new Dictionary <string, Constant> ();

            while (reader.Read ())
                result [reader.GetString (0)] = new Constant (reader.GetString (0), reader.GetInt64 (1));

            return result;
        }
    }
}
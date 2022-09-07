using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using EVESharp.EVE.Data.Configuration;
using EVESharp.PythonTypes.Database;
using EVESharp.PythonTypes.Types.Collections;
using EVESharp.PythonTypes.Types.Database;
using EVESharp.PythonTypes.Types.Primitives;

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

    public static PyList <PyObjectData> EveFetchLiveUpdates (this IDatabaseConnection Database)
    {
        try
        {
            IDbConnection connection = null;
            DbDataReader reader = Database.Select (
                ref connection,
                "SELECT updateID, updateName, description, machoVersionMin, machoVersionMax, buildNumberMin, buildNumberMax, methodName, objectID, codeType, code, OCTET_LENGTH(code) as codeLength FROM eveLiveUpdates"
            );

            using (connection)
            using (reader)
            {
                PyList <PyObjectData> result = new PyList <PyObjectData> ();

                while (reader.Read ())
                {
                    PyDictionary entry = new PyDictionary ();
                    PyDictionary code  = new PyDictionary ();

                    // read the blob for the liveupdate
                    byte [] buffer = new byte[reader.GetInt32 (11)];
                    reader.GetBytes (10, 0, buffer, 0, buffer.Length);

                    code ["code"]       = buffer;
                    code ["codeType"]   = reader.GetString (9);
                    code ["methodName"] = reader.GetString (7);
                    code ["objectID"]   = reader.GetString (8);

                    entry ["code"] = KeyVal.FromDictionary (code);

                    result.Add (KeyVal.FromDictionary (entry));
                }

                return result;
            }
        }
        catch (Exception)
        {
            throw new Exception ("Cannot prepare live-updates information for client");
        }
    }
}
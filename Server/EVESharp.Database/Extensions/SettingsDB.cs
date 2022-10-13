using System;
using System.Collections.Generic;
using System.Data.Common;
using EVESharp.Database.Types;
using EVESharp.EVE.Data.Configuration;
using EVESharp.Types;
using EVESharp.Types.Collections;

namespace EVESharp.Database.Extensions;

public static class SettingsDB
{
    public static Dictionary <string, Constant> EveLoadConstants (this IDatabase Database)
    {
        using (DbDataReader reader = Database.Select ("SELECT constantID, constantValue FROM eveConstants"))
        {
            Dictionary <string, Constant> result = new Dictionary <string, Constant> ();

            while (reader.Read ())
                result [reader.GetString (0)] = new Constant (reader.GetString (0), reader.GetInt64 (1));

            return result;
        }
    }

    public static PyList <PyObjectData> EveFetchLiveUpdates (this IDatabase Database)
    {
        try
        {
            DbDataReader reader = Database.Select (
                "SELECT updateID, updateName, description, machoVersionMin, machoVersionMax, buildNumberMin, buildNumberMax, methodName, objectID, codeType, code, OCTET_LENGTH(code) as codeLength FROM eveLiveUpdates"
            );

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
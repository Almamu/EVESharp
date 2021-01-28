/*
    ------------------------------------------------------------------------------------
    LICENSE:
    ------------------------------------------------------------------------------------
    This file is part of EVE#: The EVE Online Server Emulator
    Copyright 2021 - EVE# Team
    ------------------------------------------------------------------------------------
    This program is free software; you can redistribute it and/or modify it under
    the terms of the GNU Lesser General Public License as published by the Free Software
    Foundation; either version 2 of the License, or (at your option) any later
    version.

    This program is distributed in the hope that it will be useful, but WITHOUT
    ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
    FOR A PARTICULAR PURPOSE. See the GNU Lesser General Public License for more details.

    You should have received a copy of the GNU Lesser General Public License along with
    this program; if not, write to the Free Software Foundation, Inc., 59 Temple
    Place - Suite 330, Boston, MA 02111-1307, USA, or go to
    http://www.gnu.org/copyleft/lesser.txt.
    ------------------------------------------------------------------------------------
    Creator: Almamu
*/

using System;
using System.Collections.Generic;
using Common.Database;
using Common.Logging;
using MySql.Data.MySqlClient;
using PythonTypes.Types.Primitives;

namespace ClusterControler.Database
{
    public class GeneralDB : DatabaseAccessor
    {
        private Channel Log { get; }

        public PyList FetchLiveUpdates()
        {
            try
            {
                MySqlConnection connection = null;
                MySqlDataReader reader = Database.Query(ref connection,
                    "SELECT updateID, updateName, description, machoVersionMin, machoVersionMax, buildNumberMin, buildNumberMax, methodName, objectID, codeType, code, OCTET_LENGTH(code) as codeLength FROM eveLiveUpdates"
                );

                using (connection)
                using (reader)
                {
                    PyList result = new PyList();

                    while (reader.Read())
                    {
                        PyDictionary entry = new PyDictionary();
                        PyDictionary code = new PyDictionary();

                        // read the blob for the liveupdate
                        byte[] buffer = new byte[reader.GetUInt32(11)];
                        reader.GetBytes(10, 0, buffer, 0, buffer.Length);

                        code["code"] = buffer;
                        code["codeType"] = reader.GetString(9);
                        code["methodName"] = reader.GetString(7);
                        code["objectID"] = reader.GetString(8);

                        entry["code"] = new PyObjectData("util.KeyVal", code);

                        result.Add(
                            new PyObjectData("util.KeyVal", entry)
                        );
                    }

                    return result;
                }
            }
            catch (Exception)
            {
                Log.Error($"Cannot prepare live-updates information for client");
                throw;
            }
        }

        public GeneralDB(Logger logger, DatabaseConnection db) : base(db)
        {
            this.Log = logger.CreateLogChannel("GeneralDB");
        }
    }
}
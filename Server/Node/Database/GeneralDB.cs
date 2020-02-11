/*
    ------------------------------------------------------------------------------------
    LICENSE:
    ------------------------------------------------------------------------------------
    This file is part of EVE#: The EVE Online Server Emulator
    Copyright 2012 - Glint Development Group
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
using System.Linq;
using System.Text;
using MySql.Data.MySqlClient;
using Common;
using Common.Database;

namespace Node.Database
{
    public class GeneralDB : DatabaseAccessor
    {
        public List<int> GetUnloadedSolarSystems()
        {
            MySqlDataReader res = null;
            MySqlConnection connection = null;

            Database.Query(ref res, ref connection, "SELECT solarSystemID FROM solarsystemsloaded WHERE nodeID=0");
            
            using(connection)
            using (res)
            {
                List<int> result = new List<int>();

                while (res.Read())
                {
                    result.Add(res.GetInt32(0));
                }

                return result;   
            }
        }

        public void LoadSolarSystem(int solarSystemID)
        {
            try
            {
                Database.Query(
                    "UPDATE solarsystemsloaded SET nodeID = " + Program.NodeID + " WHERE solarSystemID = " +
                    solarSystemID
                );
            }
            catch (Exception e)
            {
                throw new Exception("Cannot change solarSystem {solarSystemID} status to loaded", e);
            }
        }

        public void UnloadSolarSystem(int solarSystemID)
        {
            Database.Query("UPDATE solarsystemsloaded SET nodeID=0 WHERE solarSystemID=" + solarSystemID);
        }

        public GeneralDB(DatabaseConnection db) : base(db)
        {
        }
    }
}

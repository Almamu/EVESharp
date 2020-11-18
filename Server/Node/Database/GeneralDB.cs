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
using Common.Database;
using MySql.Data.MySqlClient;
using Node.Inventory.SystemEntities;

namespace Node.Database
{
    public class GeneralDB : DatabaseAccessor
    {
        public List<int> GetUnloadedSolarSystems()
        {
            MySqlConnection connection = null;
            MySqlDataReader reader = Database.Query(
                ref connection, "SELECT solarSystemID FROM solarsystemsloaded WHERE nodeID = 0"
            );

            using (connection)
            using (reader)
            {
                List<int> result = new List<int>();

                while (reader.Read() == true)
                    result.Add(reader.GetInt32(0));

                return result;
            }
        }

        public void MarkSolarSystemAsLoaded(SolarSystem solarSystem)
        {
            try
            {
                Database.PrepareQuery(
                    "UPDATE solarsystemsloaded SET nodeID = @nodeID WHERE solarSystemID = @solarSystemID", new Dictionary<string, object>()
                    {
                        {"@nodeID", Program.NodeID},
                        {"@solarSystemID", solarSystem.ID}
                    }
                );
            }
            catch (Exception e)
            {
                throw new Exception("Cannot change solarSystem {solarSystemID} status to loaded", e);
            }
        }

        public void MarkSolarSystemAsUnloaded(SolarSystem solarSystem)
        {
            Database.PrepareQuery("UPDATE solarsystemsloaded SET nodeID = 0 WHERE solarSystemID = @solarSystemID", new Dictionary<string, object>()
            {
                {"@solarSystemID", solarSystem.ID}
            });
        }

        public GeneralDB(DatabaseConnection db) : base(db)
        {
        }
    }
}
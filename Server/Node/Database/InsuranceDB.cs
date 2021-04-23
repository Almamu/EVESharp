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
using MySql.Data.MySqlClient;
using PythonTypes.Types.Collections;
using PythonTypes.Types.Database;
using PythonTypes.Types.Primitives;

namespace Node.Database
{
    public class InsuranceDB : DatabaseAccessor
    {
        public PyList<PyPackedRow> GetContractsForShipsOnStation(int characterID, int stationID)
        {
            return Database.PreparePackedRowListQuery(
                "SELECT chrShipInsurances.ownerID, shipID, fraction, startDate, endDate FROM chrShipInsurances LEFT JOIN invItems ON invItems.itemID = shipID WHERE chrShipInsurances.ownerID = @characterID AND invItems.locationID = @stationID",
                new Dictionary<string, object>()
                {
                    {"@characterID", characterID},
                    {"@stationID", stationID}
                }
            );
        }
        public PyList<PyPackedRow> GetContractsForShipsOnStationIncludingCorp(int characterID, int corporationID, int stationID)
        {
            return Database.PreparePackedRowListQuery(
                "SELECT chrShipInsurances.ownerID, shipID, fraction, startDate, endDate FROM chrShipInsurances LEFT JOIN invItems ON invItems.itemID = shipID WHERE (chrShipInsurances.ownerID = @characterID OR chrShipInsurances.ownerID = @corporationID) AND invItems.locationID = @stationID",
                new Dictionary<string, object>()
                {
                    {"@characterID", characterID},
                    {"@corporationID", corporationID},
                    {"@stationID", stationID}
                }
            );
        }

        public PyPackedRow GetContractForShip(int characterID, int shipID)
        {
            return Database.PreparePackedRowQuery(
                "SELECT ownerID, shipID, fraction, startDate, endDate FROM chrShipInsurances WHERE ownerID = @characterID AND shipID = @shipID",
                new Dictionary<string, object>()
                {
                    {"@characterID", characterID},
                    {"@shipID", shipID}
                }
            );
        }

        public bool IsShipInsured(int shipID, out string ownerName, out int numberOfInsurances)
        {
            ownerName = null;
            numberOfInsurances = 0;
            
            MySqlConnection connection = null;
            MySqlDataReader reader = Database.PrepareQuery(ref connection,
                "SELECT COUNT(*) AS insuranceCount, itemName FROM chrShipInsurances LEFT JOIN eveNames ON itemID = chrShipInsurances.ownerID WHERE shipID = @shipID",
                new Dictionary<string, object>()
                {
                    {"@shipID", shipID}
                }
            );
            
            using (connection)
            using (reader)
            {
                if (reader.Read() == false)
                    return false;

                numberOfInsurances = reader.GetInt32(0);
                ownerName = reader.GetString(1);

                return true;
            }
        }

        public int InsureShip(int shipID, int characterID, double fraction, DateTime expirationDate)
        {
            // calculate the expiration date based on the game's UI, 12 weeks
            long endDate = expirationDate.ToFileTimeUtc();
            
            return (int) Database.PrepareQueryLID(
                "INSERT INTO chrShipInsurances(ownerID, shipID, fraction, startDate, endDate)VALUES(@characterID, @shipID, @fraction, @startDate, @endDate)",
                new Dictionary<string, object>()
                {
                    {"@characterID", characterID},
                    {"@shipID", shipID},
                    {"@fraction", fraction},
                    {"@startDate", DateTime.UtcNow.ToFileTimeUtc()},
                    {"@endDate", endDate}
                }
            );
        }

        public void UnInsureShip(int shipID)
        {
            Database.PrepareQuery(
                "DELETE FROM chrShipInsurances WHERE shipID = @shipID",
                new Dictionary<string, object>()
                {
                    {"@shipID", shipID}
                }
            );
        }
        
        public InsuranceDB(DatabaseConnection db) : base(db)
        {
        }
    }
}
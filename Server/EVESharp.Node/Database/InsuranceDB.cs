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
using System.Data;
using System.Data.Common;
using EVESharp.Common.Database;
using EVESharp.Node.Inventory;
using EVESharp.PythonTypes.Types.Collections;
using EVESharp.PythonTypes.Types.Database;
using Type = EVESharp.EVE.Data.Inventory.Type;

namespace EVESharp.Node.Database;

public class InsuranceDB : DatabaseAccessor
{
    private TypeManager TypeManager { get; }

    public InsuranceDB (TypeManager typeManager, IDatabaseConnection db) : base (db)
    {
        TypeManager = typeManager;
    }

    public PyList <PyPackedRow> GetContractsForShipsOnStation (int characterID, int stationID)
    {
        return Database.PreparePackedRowList (
            "SELECT chrShipInsurances.ownerID, shipID, fraction, startDate, endDate FROM chrShipInsurances LEFT JOIN invItems ON invItems.itemID = shipID WHERE chrShipInsurances.ownerID = @characterID AND invItems.locationID = @stationID",
            new Dictionary <string, object>
            {
                {"@characterID", characterID},
                {"@stationID", stationID}
            }
        );
    }

    public PyList <PyPackedRow> GetContractsForShipsOnStationIncludingCorp (int characterID, int corporationID, int stationID)
    {
        return Database.PreparePackedRowList (
            "SELECT chrShipInsurances.ownerID, shipID, fraction, startDate, endDate FROM chrShipInsurances LEFT JOIN invItems ON invItems.itemID = shipID WHERE (chrShipInsurances.ownerID = @characterID OR chrShipInsurances.ownerID = @corporationID) AND invItems.locationID = @stationID",
            new Dictionary <string, object>
            {
                {"@characterID", characterID},
                {"@corporationID", corporationID},
                {"@stationID", stationID}
            }
        );
    }

    public PyPackedRow GetContractForShip (int characterID, int shipID)
    {
        return Database.PreparePackedRow (
            "SELECT ownerID, shipID, fraction, startDate, endDate FROM chrShipInsurances WHERE ownerID = @characterID AND shipID = @shipID",
            new Dictionary <string, object>
            {
                {"@characterID", characterID},
                {"@shipID", shipID}
            }
        );
    }

    public bool IsShipInsured (int shipID, out int ownerID, out int numberOfInsurances)
    {
        ownerID            = 0;
        numberOfInsurances = 0;

        IDbConnection connection = null;
        DbDataReader reader = Database.Select (
            ref connection,
            "SELECT COUNT(*) AS insuranceCount, ownerID FROM chrShipInsurances WHERE shipID = @shipID",
            new Dictionary <string, object> {{"@shipID", shipID}}
        );

        using (connection)
        using (reader)
        {
            if (reader.Read () == false)
                return false;

            numberOfInsurances = reader.GetInt32 (0);

            if (numberOfInsurances > 0)
                ownerID = reader.GetInt32 (1);

            return numberOfInsurances > 0;
        }
    }

    public int InsureShip (int shipID, int characterID, double fraction, DateTime expirationDate)
    {
        // calculate the expiration date based on the game's UI, 12 weeks
        long endDate = expirationDate.ToFileTimeUtc ();

        return (int) Database.PrepareLID (
            "INSERT INTO chrShipInsurances(ownerID, shipID, fraction, startDate, endDate)VALUES(@characterID, @shipID, @fraction, @startDate, @endDate)",
            new Dictionary <string, object>
            {
                {"@characterID", characterID},
                {"@shipID", shipID},
                {"@fraction", fraction},
                {"@startDate", DateTime.UtcNow.ToFileTimeUtc ()},
                {"@endDate", endDate}
            }
        );
    }

    public void UnInsureShip (int shipID)
    {
        Database.Prepare (
            "DELETE FROM chrShipInsurances WHERE shipID = @shipID",
            new Dictionary <string, object> {{"@shipID", shipID}}
        );
    }

    /// <summary>
    /// WARNING: SIDE EFFECTS, CHANGES THE NOTIFICATION FLAG OF THE EXPIRED CONTRACTS FOUND
    /// </summary>
    /// <returns>All the expired contracts not notified yet to their owners</returns>
    public IEnumerable <ExpiredContract> GetExpiredContracts ()
    {
        long currentDate = DateTime.UtcNow.ToFileTimeUtc ();

        IDbConnection connection = null;
        DbDataReader reader = Database.Select (
            ref connection,
            "SELECT insuranceID, chrShipInsurances.ownerID, shipID, ship.itemName AS shipName, invItems.typeID AS shipTypeID, eveNames.typeID AS ownerTypeID, startDate FROM chrShipInsurances LEFT JOIN eveNames ON eveNames.itemID = chrShipInsurances.ownerID LEFT JOIN invItems ON invItems.itemID = chrShipInsurances.shipID LEFT JOIN eveNames ship ON eveNames.itemID = shipID WHERE endDate < @currentDate",
            new Dictionary <string, object> {{"@currentDate", currentDate}}
        );

        using (connection)
        using (reader)
        {
            List <ExpiredContract> result = new List <ExpiredContract> ();

            while (reader.Read ())
            {
                Type shipType = TypeManager [reader.GetInt32 (4)];

                yield return new ExpiredContract
                {
                    InsuranceID = reader.GetInt32 (0),
                    OwnerID     = reader.GetInt32 (1),
                    ShipID      = reader.GetInt32 (2),
                    ShipName    = reader.GetStringOrDefault (3, shipType.Name),
                    ShipType    = shipType,
                    OwnerTypeID = TypeManager [reader.GetInt32 (5)],
                    StartDate   = reader.GetInt64 (6)
                };
            }
        }

        // remove all the insurances from the database
        Database.Prepare (
            "DELETE FROM chrShipInsurances WHERE insuranceID < @currentDate",
            new Dictionary <string, object> {{"@currentDate", currentDate}}
        );
    }

    public class ExpiredContract
    {
        public int    InsuranceID { get; init; }
        public int    OwnerID     { get; init; }
        public Type   OwnerTypeID { get; init; }
        public int    ShipID      { get; init; }
        public Type   ShipType    { get; init; }
        public string ShipName    { get; init; }
        public long   StartDate   { get; init; }
    }
}
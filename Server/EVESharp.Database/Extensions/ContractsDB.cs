using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using EVESharp.Database.Inventory;
using EVESharp.Database.Inventory.Attributes;
using EVESharp.Database.Market;
using EVESharp.Types;
using EVESharp.Types.Collections;

namespace EVESharp.Database.Extensions;

public static class ContractsDB
{
    public static Contract ConGet (this DbLock DbLock, int contractID)
    {
        DbDataReader reader = DbLock.Creator.SelectProcedure (
            DbLock.Connection,
            "ConGet",
            new Dictionary <string, object>
            {
                {"_contractID", contractID}
            }
        );

        using (reader)
        {
            if (reader.Read () == false)
                return null;

            return new Contract
            {
                ID             = contractID,
                Price          = reader.GetDoubleOrNull (0),
                Collateral     = reader.GetInt32 (1),
                Status         = (ContractStatus) reader.GetInt32 (2),
                Type           = (ContractTypes) reader.GetInt32 (3),
                ExpireTime     = reader.GetInt64 (4),
                CrateID        = reader.GetInt32 (5),
                StartStationID = reader.GetInt32 (6),
                EndStationID   = reader.GetInt32OrDefault (12),
                IssuerID       = reader.GetInt32 (7),
                IssuerCorpID   = reader.GetInt32 (8),
                ForCorp        = reader.GetBoolean (9),
                Reward         = reader.GetDoubleOrNull (10),
                Volume         = reader.GetDoubleOrNull (11),
            };
        }
    }

    public static PreloadedContractItem ConGetItemPreloadValues (this DbLock DbLock, int itemID, int ownerID, int stationID)
    {
        DbDataReader reader = DbLock.Creator.SelectProcedure (
            DbLock.Connection,
            "ConGetItemPreloadValues",
            new Dictionary <string, object> ()
            {
                {"_itemID", itemID},
                {"_ownerID", ownerID},
                {"_stationID", stationID},
                {"_damage", (int) AttributeTypes.damage},
                {"_volume", (int) AttributeTypes.volume}
            }
        );

        using (reader)
        {
            if (reader.Read () == false)
                throw new InvalidDataException ("Cannot find the given item");

            return new PreloadedContractItem ()
            {
                Quantity   = reader.GetInt32 (0),
                Damage     = reader.GetDoubleOrDefault (1),
                TypeID       = reader.GetInt32 (2),
                CategoryID = reader.GetInt32 (3),
                Singleton  = reader.GetBoolean (4),
                Contraband = reader.GetBoolean (5),
                Volume     = reader.GetDouble (6)
            };
        }
    }

    public static void ConAddItem (this DbLock DbLock, int contractID, int typeID, int quantity, int inCrate, int? itemID)
    {
        DbLock.Creator.InsertProcedure (
            DbLock.Connection,
            "ConAddItem",
            new Dictionary <string, object> ()
            {
                {"_contractID", contractID},
                {"_typeID", typeID},
                {"_quantity", quantity},
                {"_inCrate", inCrate},
                {"_itemID", itemID}
            }
        );
    }

    public static void ConSaveInfo (this DbLock DbLock, Contract contract)
    {
        DbLock.Creator.QueryProcedure (
            DbLock.Connection,
            "ConSaveInfo",
            new Dictionary <string, object> ()
            {
                {"_contractID", contract.ID},
                {"_status", (int) contract.Status},
                {"_crateID", contract.CrateID},
                {"_volume", contract.Volume},
                {"_acceptorID", contract.AcceptorID},
                {"_dateAccepted", contract.AcceptedDate}
            }
        );
    }

    public static (int bidderID, int amount, int walletKey) ConGetMaximumBid (this DbLock DbLock, int contractID)
    {
        DbDataReader reader = DbLock.Creator.SelectProcedure (
            DbLock.Connection,
            "ConGetMaximumBid",
            new Dictionary <string, object> ()
            {
                {"_contractID", contractID}
            }
        );

        using (reader)
        {
            if (reader.Read () == false)
                return (0, 0, 0);

            return (
                reader.GetInt32 (0),
                reader.GetInt32 (1),
                reader.GetInt32 (2)
            );
        }
    }

    public static PyList<PyInteger> ConGetOutbids (this DbLock DbLock, int contractID)
    {
        return DbLock.Creator.List <PyInteger> (
            "ConGetOutbids",
            new Dictionary <string, object> ()
            {
                {"_contractID", contractID}
            }
        );
    }

    public static ulong ConPlaceBid (this DbLock DbLock, int contractID, int quantity, int bidder, bool forCorp, int walletKey)
    {
        return DbLock.Creator.InsertProcedure (
            DbLock.Connection,
            "ConPlaceBid",
            new Dictionary <string, object> ()
            {
                {"_contractID", contractID},
                {"_amount", quantity},
                {"_bidderID", bidder},
                {"_forCorp", forCorp},
                {"_walletKey", walletKey}
            }
        );
    }

    public static IEnumerable <(int typeID, int quantity, int? itemID)> ConGetItems (this DbLock DbLock, int contractID, int inCrate)
    {
        DbDataReader reader = DbLock.Creator.SelectProcedure (
            DbLock.Connection,
            "ConGetItems",
            new Dictionary <string, object> ()
            {
                {"_contractID", contractID},
                {"_inCrate", inCrate}
            }
        );

        using (reader)
        {
            while (reader.Read () == true)
            {
                yield return (
                    reader.GetInt32 (0),
                    reader.GetInt32 (1),
                    reader.GetInt32OrNull (2)
                );    
            }
        }
    }

    public static IEnumerable<RequestedContractItem> ConGetRequestedItemsAtLocation (this DbLock DbLock, int typeID, int locationID, int ownerID, Flags flag = Flags.Hangar)
    {
        DbDataReader reader = DbLock.Creator.SelectProcedure (
            "ConGetRequestedItemsAtLocation",
            new Dictionary <string, object> ()
            {
                {"_typeID", typeID},
                {"_ownerID", ownerID},
                {"_locationID", locationID},
                {"_flag", (int) flag},
                {"_damage", (int) AttributeTypes.damage},
            }
        );

        using (reader)
        {
            while (reader.Read () == true)
                yield return new RequestedContractItem ()
                {
                    ItemID = reader.GetInt32 (0),
                    Quantity = reader.GetInt32 (1),
                    Damage = reader.GetDoubleOrDefault (2),
                    Singleton = reader.GetBoolean (3),
                    Contraband = reader.GetBoolean (4)
                };
        }
    }

    public static void ConDestroy (this DbLock DbLock, int contractID)
    {
        DbLock.Creator.QueryProcedure (
            DbLock.Connection,
            "ConDestroy",
            new Dictionary <string, object> ()
            {
                {"_contractID", contractID}
            }
        );
    }
}
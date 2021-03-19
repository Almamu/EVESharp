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

using System.Collections.Generic;
using Common.Database;
using Node.Services.Contracts;
using PythonTypes.Types.Primitives;

namespace Node.Database
{
    public class ContractDB : DatabaseAccessor
    {
        public ContractDB(DatabaseConnection db) : base(db)
        {
        }

        public PyDataType NumRequiringAttention(int characterID, int corporationID)
        {
            return Database.PrepareKeyValQuery(
                "SELECT" +
                " (SELECT COUNT(*) FROM conContracts WHERE issuerID = @characterID AND forCorp = @notForCorp AND requiresAttentionByOwner = 1) + (SELECT COUNT(*) FROM conContracts WHERE assigneeID = @characterID AND forCorp = @notForCorp AND requiresAttentionByAssignee = 1) AS n," +
                " (SELECT COUNT(*) FROM conContracts WHERE issuerCorpID = @corporationID AND forCorp = @forCorp AND requiresAttentionByOwner = 1) + (SELECT COUNT(*) FROM conContracts WHERE assigneeID = @corporationID AND forCorp = @forCorp AND requiresAttentionByAssignee = 1) AS ncorp",
                new Dictionary<string, object>()
                {
                    {"@characterID", characterID},
                    {"@corporationID", corporationID},
                    {"@forCorp", 1},
                    {"@notForCorp", 0}
                }
            );
        }

        public PyDataType NumOutstandingContracts(int characterID, int corporationID)
        {
            return Database.PrepareKeyValQuery(
                "SELECT" +
                " (SELECT COUNT(*) FROM conContracts WHERE issuerID = @corporationID AND forCorp = @forCorp AND status = @outstandingStatus) AS myCorpTotal," +
                " (SELECT COUNT(*) FROM conContracts WHERE issuerID = @characterID AND forCorp = @notForCorp AND status = @outstandingStatus) AS myCharTotal," +
                " (SELECT COUNT(*) FROM conContracts WHERE assigneeID = @corporationID AND forCorp = @notForCorp AND status = @outstandingStatus) AS nonCorpForMyCorp," +
                " (SELECT COUNT(*) FROM conContracts WHERE assigneeID = @characterID AND forCorp = @notForCorp AND status = @outstandingStatus) AS nonCorpForMyChar",
                new Dictionary<string, object>()
                {
                    {"@characterID", characterID},
                    {"@corporationID", corporationID},
                    {"@forCorp", 1},
                    {"@notForCorp", 0},
                    {"@outstandingStatus", (int) ContractStatus.Outstanding}
                }
            );
        }

        public PyDataType CollectMyPageInfo(int characterID, int corporationID)
        {
            // TODO: PROPERLY IMPLEMENT THIS
            return Database.PrepareKeyValQuery(
                "SELECT" +
                " (SELECT COUNT(*) FROM conContracts WHERE status = @outstandingStatus AND issuerID = @characterID AND forCorp = @notForCorp) AS numOutstandingContractsNonCorp," +
                " (SELECT COUNT(*) FROM conContracts WHERE status = @outstandingStatus AND issuerCorpID = @corporationID AND forCorp = @forCorp) AS numOutstandingContractsForCorp," +
                " (SELECT COUNT(*) FROM conContracts WHERE status = @outstandingStatus AND issuerID = @characterID OR issuerCorpID = @corporationID) AS numOutstandingContracts," +
                " (SELECT COUNT(*) FROM conContracts WHERE issuerID = @characterID AND forCorp = @notForCorp AND requiresAttentionByOwner = 1) + (SELECT COUNT(*) FROM conContracts WHERE assigneeID = @characterID AND forCorp = @notForCorp AND requiresAttentionByAssignee = 1) AS numRequiresAttention," +
                " (SELECT COUNT(*) FROM conContracts WHERE issuerCorpID = @corporationID AND forCorp = @forCorp AND requiresAttentionByOwner = 1) + (SELECT COUNT(*) FROM conContracts WHERE assigneeID = @corporationID AND forCorp = @forCorp AND requiresAttentionByAssignee = 1) AS numRequiresAttentionCorp," +
                " (SELECT COUNT(*) FROM conContracts WHERE assigneeID = @characterID) AS numAssignedTo," +
                " (SELECT COUNT(*) FROM conContracts WHERE assigneeID = @corporationID) AS numAssignedToCorp," +
                " (SELECT COUNT(*) FROM conBids LEFT JOIN conContracts USING(contractID) WHERE conBids.issuerID = @characterID AND status = @outstandingStatus AND forCorp = @notForCorp) AS numBiddingOn," +
                " (SELECT COUNT(*) FROM conContracts WHERE status = @inProgressStatus AND issuerID = @characterID AND forCorp = @notForCorp) AS numInProgress," +
                " (SELECT COUNT(*) FROM conBids LEFT JOIN conContracts USING(contractID) WHERE conBids.issuerCorpID = @corporationID AND status = @outstandingStatus AND forCorp = @forCorp) AS numBiddingOnCorp," +
                " (SELECT COUNT(*) FROM conContracts WHERE status = @inProgressStatus AND issuerCorpID = @corporationID AND forCorp = @forCorp) AS numInProgressCorp",
                new Dictionary<string, object>()
                {
                    {"@outstandingStatus", (int) ContractStatus.Outstanding},
                    {"@inProgressStatus", (int) ContractStatus.InProgress},
                    {"@notForCorp", 0},
                    {"@forCorp", 1},
                    {"@corporationID", corporationID},
                    {"@characterID", characterID}
                }
            );
        }

        public PyDataType GetContractsForOwner(int characterID, int corporationID)
        {
            return Database.PrepareCRowsetQuery(
                "SELECT contractID, issuerID, issuerCorpID, type, avail, assigneeID, expiretime, duration, startStationID, endStationID, price, reward, collateral, title, description, forCorp, status, isAccepted, acceptorID, dateIssued, dateExpired, dateAccepted, dateCompleted, volume, requiresAttentionByOwner, requiresAttentionByAssignee, crateID, issuerWalletKey, issuerAllianceID, acceptorWalletKey FROM conContracts WHERE (issuerID = @characterID AND forCorp = @notForCorp) OR (issuerCorpID = @corporationID AND forCorp = @forCorp)",
                new Dictionary<string, object>()
                {
                    {"@characterID", characterID},
                    {"@corporationID", corporationID},
                    {"@notForCorp", 0},
                    {"@forCorp", 1}
                }
            );
        }

        public PyDataType GetContractBidsForOwner(int characterID, int corporationID)
        {
            return Database.PrepareRowsetQuery(
                "SELECT bidID, contractID, conBids.issuerID, quantity, conBids.issuerCorpID, issuerStationID FROM conBids WHERE issuerID = @characterID OR issuerCorpID = @corporationID",
                new Dictionary<string, object>()
                {
                    {"@characterID", characterID},
                    {"@corporationID", corporationID}
                }
            );
        }

        public PyDataType GetContractItemsForOwner(int characterID, int corporationID)
        {
            return Database.PrepareRowsetQuery(
                "SELECT itemTypeID, quantity, inCrate FROM conItems LEFT JOIN conContracts USING (contractID) WHERE (issuerID = @characterID AND forCorp = @notForCorp) OR (issuerCorpID = @corporationID AND forCorp = @forCorp)",
                new Dictionary<string, object>()
                {
                    {"@characterID", characterID},
                    {"@corporationID", corporationID},
                    {"@notForCorp", 0},
                    {"@forCorp", 1}
                }
            );
        }
    }
}
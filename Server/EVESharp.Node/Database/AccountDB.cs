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
using System.Security.Cryptography;
using System.Text;
using EVESharp.Common.Database;
using MySql.Data.MySqlClient;

namespace EVESharp.Node.Database
{
    public class AccountDB : DatabaseAccessor
    {
        public bool AccountExists(string username)
        {
            MySqlConnection connection = null;
            MySqlDataReader reader = Database.Select(ref connection,
                "SELECT COUNT(accountID) FROM account WHERE accountName = @username",
                new Dictionary<string, object>()
                {
                    {"@username", username}
                }
            );

            using (connection)
            using (reader)
            {
                //If any errors getting account count from database return account exists.
                if (reader.FieldCount != 1)
                    return true;
                
                if (reader.Read() == false)
                    return true;
                
                return (reader.GetInt64(0) > 0);
            }
        }

        public bool LoginPlayer(string username, string password, ref int accountid, ref bool banned, ref ulong role)
        {
            MySqlConnection connection = null;
            MySqlDataReader reader = Database.Select(ref connection,
                "SELECT accountID, password, banned, role FROM account WHERE accountName LIKE @username AND password LIKE SHA1(@password)",
                new Dictionary<string, object>()
                {
                    {"@username", username},
                    {"@password", password}
                }
            );

            using (connection)
            {
                using (reader)
                {
                    if (reader.FieldCount == 0)
                        return false;

                    if (reader.Read() == false)
                        return false;

                    accountid = reader.GetInt32(0);
                    banned = reader.GetBoolean(2);
                    role = reader.GetUInt64(3);

                    return true;
                }
            }
        }

        public void CreateAccount(string name, string password, ulong role)
        {
            Database.PrepareQuery(
                "INSERT INTO account(accountID, accountName, password, role, online, banned)VALUES(NULL, @accountName, SHA1(@password), @role, 0, 0)",
                new Dictionary<string, object>()
                {
                    {"@accountName", name},
                    {"@password", password},
                    {"@role", role}
                }
            );
        }

        public int GetAccountIDFromCharacterID(int characterID)
        {
            MySqlConnection connection = null;
            MySqlDataReader reader = Database.Select(ref connection,
                "SELECT accountID FROM chrInformation WHERE characterID = @characterID AND online = 1",
                new Dictionary<string, object>()
                {
                    {"@characterID", characterID}
                }
            );
            
            using (connection)
            using (reader)
            {
                if (reader.Read() == false)
                    throw new ArgumentOutOfRangeException(nameof(characterID),"Unknown characterID or characterID not online");

                return reader.GetInt32(0);
            }
        }

        public AccountDB(DatabaseConnection db) : base(db)
        {
        }
    }
}
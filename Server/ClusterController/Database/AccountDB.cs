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
using System.Data.SqlTypes;
using System.Security.Cryptography;
using Common;
using Common.Database;
using MySql.Data.MySqlClient;
using MySql.Data.Types;

namespace ClusterControler.Database
{
    public class AccountDB : DatabaseAccessor
    {
        public bool AccountExists(string username)
        {
            MySqlDataReader reader = null;
            MySqlConnection connection = null;

            username = Database.DoEscapeString(username);

            Database.Query(
                ref reader, ref connection,
                "SELECT COUNT(accountID) FROM account WHERE accountName = '" + username + "'"
            );

            using (connection)
            {
                using (reader)
                {
                    if (reader.FieldCount > 0)
                        return true;

                    return false;
                }
            }
        }

        public bool LoginPlayer(string username, string password, ref long accountid, ref bool banned, ref long role)
        {
            MySqlDataReader reader = null;
            MySqlConnection connection = null;

            Database.Query(ref reader, ref connection,
                "SELECT accountID, password, banned, role FROM account WHERE accountName LIKE '" +
                Database.DoEscapeString(username) + "' AND password LIKE SHA1('" + Database.DoEscapeString(password) + "')"
            );

            using (connection)
            {
                using (reader)
                {
                    if (reader.FieldCount == 0)
                        return false;

                    if (reader.Read() == false)
                        return false;
                    
                    accountid = reader.GetInt64(0);
                    banned = reader.GetBoolean(2);
                    role = reader.GetInt64(3);

                    return true;
                }
            }
        }

        public void CreateAccount(string accountName, string accountPassword, int role)
        {
            accountName = Database.DoEscapeString(accountName);

            Database.Query("INSERT INTO account(accountID, accountName, password, role, online, banned)" +
                "VALUES(NULL, '" + accountName + "', SHA1('" + Database.DoEscapeString(accountPassword) + "'), " +
                role + ", 0, 0)");
        }

        public AccountDB(Common.Database.DatabaseConnection db) : base(db)
        {
        }
    }
}

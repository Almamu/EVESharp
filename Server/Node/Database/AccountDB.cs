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
using System.Security.Cryptography;
using System.Text;
using Common.Database;
using MySql.Data.MySqlClient;

namespace Node.Database
{
    public class AccountDB : DatabaseAccessor
    {
        public bool LoginPlayer(string username, string password, ref long accountid, ref bool banned, ref long role)
        {
            MySqlConnection connection = null;
            MySqlDataReader reader = Database.PrepareQuery(
                ref connection,
                "SELECT accountID, password, banned, role FROM account WHERE accountName = @username",
                new Dictionary<string, object>()
                {
                    {"@username", username}
                }
            );

            using (connection)
            using (reader)
            {
                if (reader.FieldCount == 0)
                    return false;

                if (reader.Read() == false)
                    return false;

                accountid = reader.GetInt64(0);
                banned = reader.GetBoolean(2);
                role = reader.GetInt64(3);

                SHA1 sha1 = SHA1.Create();
                sha1.Initialize();
                byte[] hash = sha1.ComputeHash(Encoding.ASCII.GetBytes(password));
                byte[] outb = new byte[hash.Length];

                reader.GetBytes(1, 0, outb, 0, outb.Length);
                reader.Close();

                bool equals = true;

                for (int i = 0; i < outb.Length; i++)
                {
                    if (outb[i] != hash[i])
                    {
                        equals = false;
                        break;
                    }
                }

                return equals;
            }
        }

        public int GetAccountIDFromCharacterID(int characterID)
        {
            MySqlConnection connection = null;
            MySqlDataReader reader = Database.PrepareQuery(ref connection,
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
                    throw new ArgumentOutOfRangeException("Unknown characterID or characterID not online");

                return reader.GetInt32(0);
            }
        }

        public AccountDB(DatabaseConnection db) : base(db)
        {
        }
    }
}
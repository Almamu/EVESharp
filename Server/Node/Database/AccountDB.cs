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
using System.Security.Cryptography;

using MySql.Data.MySqlClient;

namespace EVESharp.Database
{
    public static class AccountDB
    {
        public static bool LoginPlayer(string username, string password, ref int accountid, ref bool banned, ref int role)
        {
            MySqlDataReader res = null;

            if (Database.Query(ref res, "SELECT password, accountID, banned, role FROM account WHERE accountName='" + username + "';") == false)
            {
                if (res != null) res.Close();
                return false;
            }

            if (res == null)
            {
                return false;
            }

            if (res.Read() == false)
            {
                res.Close();
                return false;
            }

            SHA1 sha1 = SHA1.Create();
            sha1.Initialize();
            byte[] hash = sha1.ComputeHash(Encoding.ASCII.GetBytes(password));
            byte[] outb = new byte[20];

            res.GetBytes(0, 0, outb, 0, 20);

            bool equals = true;

            for (int i = 0; i < 20; i++)
            {
                if (outb[i] != hash[i])
                {
                    equals = false;
                    break;
                }
            }

            if (!equals)
            {
                res.Close();
                return false;
            }

            accountid = res.GetInt32(1);
            banned = res.GetBoolean(2);
            role = res.GetInt32(3);
            res.Close();

            return true;
        }
    }
}

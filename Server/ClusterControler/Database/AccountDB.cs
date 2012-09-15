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

namespace EVESharp.ClusterControler.Database
{
    public static class AccountDB
    {
        public static bool AccountExists(string username)
        {
            var user = from h in Database.context.accounts where h.accountName.ToLower() == username.ToLower() select h;

            if (user.Count() < 1)
            {
                return false;
            }

            return true;
        }

        public static bool LoginPlayer(string username, string password, ref long accountid, ref bool banned, ref long role)
        {
            var user = from h in Database.context.accounts where h.accountName.ToLower() == username.ToLower() select h;

            if (user.Count() < 1)
            {
                return false;
            }

            SHA1 sha1 = SHA1.Create();
            sha1.Initialize();
            byte[] hash = sha1.ComputeHash(Encoding.ASCII.GetBytes(password));
            byte[] outb = user.First().password.ToArray();

            bool equals = true;

            for (int i = 0; i < user.First().password.Length; i++)
            {
                if (outb[i] != hash[i])
                {
                    equals = false;
                    break;
                }
            }

            if (equals == false)
            {
                return false;
            }

            accountid = user.First().accountID;
            banned = user.First().banned;
            role = user.First().role;

            return true;
        }

        public static void CreateAccount(string accountName, string accountPassword)
        {
            SHA1 sha1 = SHA1.Create();
            sha1.Initialize();
            byte[] hash = sha1.ComputeHash(Encoding.ASCII.GetBytes(accountPassword));

            var user = new account();

            user.accountID = Database.context.accounts.Count() + 1;
            user.accountName = accountName;
            user.banned = false;
            user.online = 0;
            user.password = hash;
            user.role = (long)(Common.Constants.Roles.ROLE_PLAYER);

            Database.context.accounts.InsertOnSubmit(user);
        }
    }
}

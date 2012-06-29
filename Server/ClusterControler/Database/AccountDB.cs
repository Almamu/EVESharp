using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MySql.Data.MySqlClient;
using System.Security.Cryptography;

namespace EVESharp.ClusterControler.Database
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

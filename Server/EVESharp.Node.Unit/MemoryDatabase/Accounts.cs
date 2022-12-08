using System.Collections.Generic;
using EVESharp.Database.Account;

namespace EVESharp.Node.Unit.MemoryDatabase;

public static class Accounts
{
    public class Entity
    {
        public int    accountID;
        public ulong  role;
        public string username;
        public string password;
        public bool   banned;
    }

    public static List <Entity> Data = new List <Entity> ()
    {
        new Entity
        {
            accountID = 1,
            username = "Almamu",
            password = "Password",
            role = (ulong) Roles.ROLE_PLAYER | (ulong) Roles.ROLE_LOGIN | (ulong) Roles.ROLE_ADMIN | (ulong) Roles.ROLE_QA | (ulong) Roles.ROLE_SPAWN |
                   (ulong) Roles.ROLE_GML | (ulong) Roles.ROLE_GDL | (ulong) Roles.ROLE_GDH | (ulong) Roles.ROLE_HOSTING | (ulong) Roles.ROLE_PROGRAMMER,
            banned = false,
        },
        new Entity
        {
            accountID = 2,
            username = "Kira",
            password = "Kira",
            role = (ulong) Roles.ROLE_PLAYER,
            banned = false,
        },
        new Entity
        {
            accountID = 3,
            username = "Banned",
            password = "Banned",
            role = (ulong) Roles.ROLE_PLAYER,
            banned = true,
        },
    };
}
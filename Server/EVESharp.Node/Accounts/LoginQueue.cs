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
using EVESharp.Common.Database;
using EVESharp.Common.Network.Messages;
using EVESharp.EVE.Packets;
using EVESharp.Node.Configuration;
using EVESharp.Node.Database;
using EVESharp.Node.Network;
using Serilog;

namespace EVESharp.Node.Accounts;

public class LoginQueue : MessageProcessor <LoginQueueEntry>
{
    private AccountDB          AccountDB     { get; }
    private Authentication     Configuration { get; }
    private DatabaseConnection Database      { get; }

    public LoginQueue (DatabaseConnection databaseConnection, Authentication configuration, AccountDB db, ILogger logger)
        : base (logger, 5)
    {
        // login should not be using too many processes
        Database      = databaseConnection;
        Configuration = configuration;
        AccountDB     = db;
    }

    public void Enqueue (MachoUnauthenticatedTransport transport, AuthenticationReq req)
    {
        this.Enqueue (
            new LoginQueueEntry
            {
                Connection = transport,
                Request    = req
            }
        );
    }

    protected override void HandleMessage (LoginQueueEntry entry)
    {
        Log.Debug ("Processing login for " + entry.Request.user_name);
        LoginStatus status = LoginStatus.Waiting;

        int   accountID = 0;
        bool  banned    = false;
        ulong role      = 0;

        // First check if the account exists
        if (AccountDB.AccountExists (entry.Request.user_name) == false)
            if (Configuration.Autoaccount)
            {
                Log.Information ($"Auto account enabled, creating account for user {entry.Request.user_name}");

                // Create the account
                AccountDB.CreateAccount (entry.Request.user_name, entry.Request.user_password, (ulong) Configuration.Role);
            }

        if (AccountDB.LoginPlayer (entry.Request.user_name, entry.Request.user_password, ref accountID, ref banned, ref role) == false || banned)
        {
            Log.Verbose (": Rejected by database");

            status = LoginStatus.Failed;
        }
        else
        {
            Log.Verbose (": success");

            status = LoginStatus.Success;

            // register player to the new address
            Database.Procedure (
                EVESharp.Database.AccountDB.REGISTER_CLIENT_ADDRESS,
                new Dictionary <string, object>
                {
                    {"_clientID", accountID},
                    {"_proxyNodeID", entry.Connection.MachoNet.NodeID}
                }
            );
        }

        entry.Connection.SendLoginNotification (status, accountID, role);
    }
}
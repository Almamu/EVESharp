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

using EVESharp.Database;
using EVESharp.EVE.Accounts;
using EVESharp.EVE.Messages.Queue;
using EVESharp.Node.Configuration;
using EVESharp.PythonTypes.Types.Database;
using Serilog;

namespace EVESharp.Node.Accounts;

public class LoginQueue : MessageQueue <LoginQueueEntry>
{
    private Authentication      Configuration { get; }
    private IDatabaseConnection Database      { get; }
    private ILogger             Log           { get; }

    public LoginQueue (IDatabaseConnection databaseConnection, Authentication configuration, ILogger logger)
    {
        // login should not be using too many processes
        this.Database      = databaseConnection;
        this.Configuration = configuration;
        this.Log           = logger;
    }

    public override void HandleMessage (LoginQueueEntry entry)
    {
        this.Log.Debug ("Processing login for " + entry.Request.user_name);
        LoginStatus status = LoginStatus.Waiting;

        // make some accommodations for the auto-account mechanism
        if (this.Database.ActExists (entry.Request.user_name) == false && this.Configuration.Autoaccount)
        {
            this.Log.Information ($"Auto account enabled, creating account for user {entry.Request.user_name}");

            // create the account
            this.Database.ActCreate (entry.Request.user_name, entry.Request.user_password, (ulong) this.Configuration.Role);
        }

        if (this.Database.ActLogin (entry.Request.user_name, entry.Request.user_password, out int? accountID, out ulong? role, out bool? banned) == false ||
            banned == true)
        {
            this.Log.Verbose (": Rejected by database");

            status = LoginStatus.Failed;
        }
        else
        {
            this.Log.Verbose (": success");

            status = LoginStatus.Success;

            // register player to the new address
            this.Database.CluRegisterClientAddress ((int) accountID, entry.Connection.MachoNet.NodeID);
        }

        entry.Connection.SendLoginNotification (status, (int) accountID, (ulong) role);
    }
}
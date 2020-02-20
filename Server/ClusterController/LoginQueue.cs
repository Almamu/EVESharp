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
using System.Threading;
using ClusterControler.Configuration;
using ClusterControler.Database;
using Common.Logging;
using Common.Packets;

namespace ClusterControler
{
    public class LoginQueue
    {
        private readonly Queue<LoginQueueEntry> Queue = new Queue<LoginQueueEntry>();
        private readonly Thread mThread = null;
        private readonly Common.Database.DatabaseConnection m_mDatabaseConnection = null;
        private readonly AccountDB mAccountDB = null;
        private readonly Authentication mConfiguration = null;
        private Channel Log { get; set; }

        public void Start()
        {
            this.mThread.Start();
        }

        public void Enqueue(ClientConnection connection, AuthenticationReq request)
        {
            // Just to be thread safe
            lock (this.Queue)
            {
                LoginQueueEntry entry = new LoginQueueEntry();

                entry.Connection = connection;
                entry.Request = request;

                this.Queue.Enqueue(entry);
            }
        }

        public void HandleLogin(LoginQueueEntry entry)
        {
            Log.Debug("Processing login for " + entry.Request.user_name);
            LoginStatus status = LoginStatus.Waiting;

            long accountID = 0;
            bool banned = false;
            long role = 0;

            // First check if the account exists
            if (this.mAccountDB.AccountExists(entry.Request.user_name) == false)
                if (this.mConfiguration.Autoaccount == true)
                {
                    Log.Info($"Auto account enabled, creating account for user {entry.Request.user_name}");

                    // Create the account
                    this.mAccountDB.CreateAccount(entry.Request.user_name, entry.Request.user_password, (ulong) this.mConfiguration.Role);                    
                }

            if ((this.mAccountDB.LoginPlayer(entry.Request.user_name, entry.Request.user_password, ref accountID, ref banned, ref role) == false) || (banned == true))
            {
                Log.Trace(": Rejected by database");

                status = LoginStatus.Failed;
            }
            else
            {
                Log.Trace(": success");

                status = LoginStatus.Sucess;
            }

            entry.Connection.SendLoginNotification(status, accountID, role);
        }

        private void Run()
        {
            // We should never exit from this loop
            try
            {
                while (true)
                {
                    Thread.Sleep(1);

                    if (Environment.HasShutdownStarted)
                        // This will shutdown all the nodes
                        // throw new ProxyClosingException();
                        return;

                    if (this.Queue.Count == 0)
                        continue;

                    // Thread safe stuff
                    lock (this.Queue)
                    {
                        LoginQueueEntry now = this.Queue.Dequeue();

                        if (now == null) 
                            continue;

                        HandleLogin(now);
                    }
                }
            }
            catch (ThreadAbortException)
            {
                // throw new ProxyClosingException();
                return;
            }
            /*catch (ProxyClosingException)
            {
                throw new ProxyClosingException();
            }*/
            catch (Exception ex)
            {
                Log.Error("Unhandled exception... " + ex.Message);
                Log.Error("Stack trace: " + ex.StackTrace);
            }

            Log.Error("LoginQueue is closing... Bye Bye!");
        }

        public LoginQueue(Authentication configuration, Common.Database.DatabaseConnection db, Logger logger)
        {
            this.Log = logger.CreateLogChannel("LoginQueue");
            this.mConfiguration = configuration;
            this.m_mDatabaseConnection = db;
            this.mAccountDB = new AccountDB(this.m_mDatabaseConnection);
            this.mThread = new Thread(Run);
        }

        public int Count()
        {
            return this.Queue.Count;
        }
    }
}
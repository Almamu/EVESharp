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
using System.Threading;

using Common;
using Common.Packets;
using Common.Network;
using ClusterControler.Database;
using Marshal;

namespace ClusterControler
{
    public class LoginQueue
    {
        public Queue<LoginQueueEntry> mQueue = new Queue<LoginQueueEntry>();
        private Thread mThread = null;
        private Common.Database.DatabaseConnection m_mDatabaseConnection = null;
        private AccountDB mAccountDB = null;

        public void Start()
        {
            this.mThread.Start();
        }

        public void Enqueue(Connection con, AuthenticationReq packet)
        {
            // Just to be thread safe
            lock (this.mQueue)
            {
                LoginQueueEntry entry = new LoginQueueEntry();

                entry.connection = con;
                entry.request = packet;

                this.mQueue.Enqueue(entry);
            }
        }


        public void HandleLogin(LoginQueueEntry entry)
        {
            Log.Debug("LoginQueue", "Processing login for " + entry.request.user_name);
            LoginStatus status = LoginStatus.Waiting;

            long accountID = 0;
            bool banned = false;
            long role = 0;

            // First check if the account exists
            if (this.mAccountDB.AccountExists(entry.request.user_name) == false)
            {
                // Create the account
                this.mAccountDB.CreateAccount(entry.request.user_name, entry.request.user_password);
            }

            if ((this.mAccountDB.LoginPlayer(entry.request.user_name, entry.request.user_password, ref accountID, ref banned, ref role) == false) || (banned == true))
            {
                Log.Trace("LoginQueue", ": Rejected by database");

                status = LoginStatus.Failed;
            }
            else
            {
                Log.Trace("LoginQueue", ": success");

                // Fill the class with the required data
                entry.connection.AccountID = accountID;
                entry.connection.Banned = banned;
                entry.connection.Role = role;
                entry.connection.LanguageID = entry.request.user_languageid;

                status = LoginStatus.Sucess;
            }
            
            entry.connection.SendLoginNotification(status);
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
                    {
                        // This will shutdown all the nodes
                        // throw new ProxyClosingException();
                        return;
                    }

                    if (this.mQueue.Count == 0)
                    {
                        continue;
                    }

                    // Thread safe stuff
                    lock (this.mQueue)
                    {
                        LoginQueueEntry now = this.mQueue.Dequeue();

                        if (now == null)
                        {
                            continue;
                        }

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
                Log.Error("LoginQueue", "Unhandled exception... " + ex.Message);
                Log.Error("ExceptionHandler", "Stack trace: " + ex.StackTrace);
            }

            Log.Error("LoginQueue", "LoginQueue is closing... Bye Bye!");
        }

        public LoginQueue(Common.Database.DatabaseConnection db)
        {
            this.m_mDatabaseConnection = db;
            this.mAccountDB = new AccountDB(this.m_mDatabaseConnection);
            this.mThread = new Thread(Run);
        }
    }
}

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

using Marshal;

namespace EVESharp.ClusterControler
{
    public static class LoginQueue
    {
        public static Queue<LoginQueueEntry> queue = new Queue<LoginQueueEntry>();
        private static Thread thread = new Thread(Run);

        public static void Start()
        {
            thread.Start();
        }

        public static void Enqueue(Connection con, AuthenticationReq packet)
        {
            // Just to be thread safe
            lock (queue)
            {
                LoginQueueEntry entry = new LoginQueueEntry();

                entry.connection = con;
                entry.request = packet;

                queue.Enqueue(entry);
            }
        }


        public static void HandleLogin(LoginQueueEntry entry)
        {
            Log.Debug("LoginQueue", "Login try " + entry.request.user_name);
            LoginStatus status = LoginStatus.Waiting;

            long accountID = 0;
            bool banned = false;
            long role = 0;

            // First check if the account exists
            if (Database.AccountDB.AccountExists(entry.request.user_name) == false)
            {
                // Create the account
                Database.AccountDB.CreateAccount(entry.request.user_name, entry.request.user_password);
            }

            if ((Database.AccountDB.LoginPlayer(entry.request.user_name, entry.request.user_password, ref accountID, ref banned, ref role) == false) || (banned == true))
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

            TCPHandler.SendLoginNotification(status, entry.connection);
        }

        private static void Run()
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

                    if (queue.Count == 0)
                    {
                        continue;
                    }

                    // Thread safe stuff
                    lock (queue)
                    {
                        LoginQueueEntry now = queue.Dequeue();

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
    }
}

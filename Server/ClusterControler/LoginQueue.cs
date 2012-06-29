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

            int accountID = 0;
            bool banned = false;
            int role = 0;

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

                    LoginQueueEntry now = queue.Dequeue();

                    if (now == null)
                    {
                        continue;
                    }

                    HandleLogin(now);
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

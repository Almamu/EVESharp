using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Common;
using Common.Packets;
using Common.Network;
using Marshal;

namespace Proxy
{
    public static class LoginQueue
    {
        public static Queue<Connection> queue = new Queue<Connection>();
        private static Thread thread = new Thread(Run);

        public static void Start()
        {
            thread.Start();
        }

        public static void Enqueue(Connection con)
        {
            // Just to be thread safe
            lock (queue)
            {
                queue.Enqueue(con);
            }
        }


        public static void HandleLogin(Connection from)
        {
            Log.Debug("LoginQueue", "Login try " + from.GetAuthenticationReq().user_name);
            Connection.LoginStatus loginStatus = Connection.LoginStatus.Waiting;

            if ((Database.AccountDB.LoginPlayer(from.GetAuthenticationReq().user_name, from.GetAuthenticationReq().user_password, ref from.accountid, ref from.banned, ref from.role) == false) || (from.banned == true))
            {
                Log.Trace("LoginQueue", ": Rejected by database");

                loginStatus = Connection.LoginStatus.Failed;
            }
            else
            {
                Log.Trace("LoginQueue", ": success");

                loginStatus = Connection.LoginStatus.Sucess;
            }

            from.SendLoginNotification(loginStatus);
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
                        throw new ProxyClosingException();
                    }

                    if (queue.Count == 0)
                    {
                        continue;
                    }

                    Connection now = queue.Dequeue();

                    if (now == null)
                    {
                        continue;
                    }

                    HandleLogin(now);
                }
            }
            catch (ThreadAbortException)
            {
                throw new ProxyClosingException();
            }
            catch (ProxyClosingException)
            {
                throw new ProxyClosingException();
            }
            catch (Exception ex)
            {
                Log.Error("LoginQueue", "Unhandled exception... " + ex.Message);
                Log.Error("ExceptionHandler", "Stack trace: " + ex.StackTrace);
            }

            Log.Error("LoginQueue", "LoginQueue is closing... Bye Bye!");
        }
    }
}

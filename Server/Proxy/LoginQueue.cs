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
            queue.Enqueue(con);
        }

        public static void HandleLogin(Connection from)
        {
            Log.Debug("Client", "Login try " + from.GetAuthenticationReq().user_name);

            if ((Database.AccountDB.LoginPlayer(from.GetAuthenticationReq().user_name, from.GetAuthenticationReq().user_password, ref from.accountid, ref from.banned, ref from.role) == false) || (from.banned == true))
            {
                Log.Trace("Client", " Rejected by database");

                from.SetLoginStatus(Connection.LoginStatus.Failed);
            }
            else
            {
                from.SetLoginStatus(Connection.LoginStatus.Sucess);
            }
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
            catch (ProxyClosingException ex)
            {
                Log.Error("LoginQueue", ex.Message);
            }
            catch (Exception)
            {

            }
        }
    }
}

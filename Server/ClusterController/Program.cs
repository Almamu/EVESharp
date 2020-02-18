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
using System.Net.Sockets;
using System.Net;
using System.IO;

using Common;
using Common.Configuration;
using Common.Constants;
using Common.Database;
using Common.Game;
using Common.Logging;
using Common.Logging.Streams;
using Common.Network;
using Common.Packets;
using Configuration;
using MySql.Data.MySqlClient;
using PythonTypes;
using PythonTypes.Marshal;
using PythonTypes.Types.Network;
using PythonTypes.Types.Primitives;

namespace ClusterControler
{
    class Program
    {
        private static DatabaseConnection sDatabase = null;
        private static General sConfiguration = null;
        private static LoginQueue sLoginQueue = null;
        private static ConnectionManager sConnectionManager = null;
        private static Logger sLog = null;
        private static Channel sChannel = null;
        private static EVEServerSocket sServerSocket = null;
        
        static AsyncCallback acceptAsync = new AsyncCallback(AcceptAsync);

        static void AcceptAsync(IAsyncResult ar)
        {
            sChannel.Trace("Incoming connection");
            EVEServerSocket serverSocket = ar.AsyncState as EVEServerSocket;
            EVEClientSocket clientSocket = serverSocket.EndAccept(ar);
            
            // put the server in accept state again
            serverSocket.BeginAccept(acceptAsync);

            sConnectionManager.AddUnauthenticatedConnection(clientSocket);
        }

        static void Main(string[] args)
        {
            try
            {
                // setup logging
                sLog = new Logger();
                // initialize main logging channel
                sChannel = sLog.CreateLogChannel("main");
                // add console log streams
                sLog.AddLogStream(new ConsoleLogStream());
                
                // load server's configuration
                sConfiguration = General.LoadFromFile("configuration.conf");
                
                // update logger's configuration
                sLog.SetConfiguration(sConfiguration.Logging);

                if (sConfiguration.LogLite.Enabled == true)
                    sLog.AddLogStream(new LogLiteStream("ClusterController", sLog, sConfiguration.LogLite));
                if (sConfiguration.FileLog.Enabled == true)
                    sLog.AddLogStream(new FileLogStream(sConfiguration.FileLog));

                // run a thread for log flushing
                new Thread(() => 
                {
                    while (true)
                    {
                        sLog.Flush();
                        Thread.Sleep(1);
                    }
                }).Start();
                
                sChannel.Info("Initializing EVESharp Cluster Controler and Proxy");
                sChannel.Fatal("Initializing EVESharp Cluster Controler and Proxy");
                sChannel.Error("Initializing EVESharp Cluster Controler and Proxy");
                sChannel.Warning("Initializing EVESharp Cluster Controler and Proxy");
                sChannel.Debug("Initializing EVESharp Cluster Controler and Proxy");
                sChannel.Trace("Initializing EVESharp Cluster Controler and Proxy");
                
                sDatabase = DatabaseConnection.FromConfiguration(sConfiguration.Database, sLog);
                sDatabase.Query("SET global max_allowed_packet=1073741824");
                sLoginQueue = new LoginQueue(sConfiguration.Authentication, sDatabase, sLog);
                sLoginQueue.Start();
                
                sConnectionManager = new ConnectionManager(sLoginQueue, sDatabase, sLog);
                
                try
                {
                    sChannel.Trace("Initializing server socket on port 26000...");
                    sServerSocket = new EVEServerSocket(26000, sLog.CreateLogChannel("ServerSocket"));
                    sServerSocket.Listen();
                    sServerSocket.BeginAccept(acceptAsync);
                    sChannel.Debug("Waiting for incoming connections on port 26000");
                }
                catch (Exception e)
                {
                    sChannel.Error($"Error listening on port 26000: {e.Message}");
                    throw;
                }
                
                while (true)
                {
                    Thread.Sleep(1);
                }
            }
            catch (Exception e)
            {
                sChannel?.Fatal(e.ToString());
                sLog?.Flush();
            }
        }
    }
}

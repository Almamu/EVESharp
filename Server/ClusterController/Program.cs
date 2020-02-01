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
using Common.Database;
using Common.Network;
using Configuration;
using Marshal;

namespace ClusterControler
{
    class Program
    {
        private static DatabaseConnection sDatabase = null;
        private static General sConfiguration = null;
        private static LoginQueue sLoginQueue = null;
        private static ConnectionManager sConnectionManager = null;
        
        private static TCPSocket sListeningSocket = new TCPSocket(26000, false);
        
        static AsyncCallback acceptAsync = new AsyncCallback(AcceptAsync);

        static void AcceptAsync(IAsyncResult ar)
        {
            TCPSocket handler = (TCPSocket)(ar.AsyncState);

            Socket sock = handler.Socket.EndAccept(ar);

            Log.Debug("Cluster", "Incoming connection");

            AsyncState state = new AsyncState();

            handler.Socket.BeginAccept(acceptAsync, handler);

            sConnectionManager.AddConnection(sock);
        }

        static void Main(string[] args)
        {
            Log.Init("cluster");

            Log.Info("Cluster", "Starting GameCluster");

            Log.Info("Cluster", "Loading database.conf file");

            sConfiguration = General.LoadFromFile("configuration.conf");
                
            // update the loglevel with the new value
            Log.SetLogLevel(sConfiguration.Logging.LogLevel);

            Log.Trace("Database", "Connecting to database...");

            sDatabase = DatabaseConnection.FromConfiguration(sConfiguration.Database);

            Log.Info("Main", "Connection to the DB sucessfull");
            Log.Debug("Cluster", "Connected to database");

            Log.Trace("Main", "Initializing login queue...");
            sLoginQueue = new LoginQueue(sDatabase);
            sLoginQueue.Start();
            Log.Info("Main", "Login queue initialized");
            
            Log.Trace("Main", "Initializing connection manager...");
            sConnectionManager = new ConnectionManager(sLoginQueue);
            Log.Info("Main", "Connection manager initialized");
            
            if (sListeningSocket.Listen(1) == false)
            {
                Log.Error("Cluster", "Cannot listen on port 26000");
                while (true) Thread.Sleep(1);
            }

            // set max_allowed_packet value to 1GB
            sDatabase.Query("SET global max_allowed_packet=1073741824");

            Log.Debug("Cluster", "Listening on port 26000");

            // Begin accept
            sListeningSocket.Socket.BeginAccept(acceptAsync, sListeningSocket);

            while (true)
            {
                // Timers or whatever here? Or maybe async calls with timers? Pretty much that will help
                Thread.Sleep(1);
            }
        }
    }
}

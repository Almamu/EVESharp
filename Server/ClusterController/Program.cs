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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using ClusterController.Configuration;
using ClusterController.Database;
using Common.Constants;
using Common.Database;
using Common.Logging;
using Common.Logging.Streams;
using Common.Network;
using Configuration;
using PythonTypes.Types.Collections;
using PythonTypes.Types.Primitives;
using SimpleInjector;

namespace ClusterController
{
    class Program
    {
        private static General sConfiguration = null;
        private static ConnectionManager sConnectionManager = null;
        private static Logger sLog = null;
        private static Channel sChannel = null;
        private static EVEServerSocket sServerSocket = null;
        private static Container sContainer = null;

        static readonly AsyncCallback acceptAsync = new AsyncCallback(AcceptAsync);

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
                sContainer = new Container();

                sContainer.Register<Logger>(Lifestyle.Singleton);
                sContainer.Register<DatabaseConnection>(Lifestyle.Singleton);
                sContainer.Register<LoginQueue>(Lifestyle.Singleton);
                sContainer.Register<ConnectionManager>(Lifestyle.Singleton);
                sContainer.Register<SystemManager>(Lifestyle.Singleton);
                sContainer.RegisterInstance(General.LoadFromFile("configuration.conf", sContainer));
                
                sContainer.Register<AccountDB>(Lifestyle.Singleton);
                sContainer.Register<GeneralDB>(Lifestyle.Singleton);
                sContainer.Register<SolarSystemDB>(Lifestyle.Singleton);
                // disable auto-verification on the container as it triggers creation of instances before they're needed
                sContainer.Options.EnableAutoVerification = false;
                
                // setup logging
                sLog = sContainer.GetInstance<Logger>();
                // initialize main logging channel
                sChannel = sLog.CreateLogChannel("main");
                // add console log streams
                sLog.AddLogStream(new ConsoleLogStream());

                // load server's configuration
                sConfiguration = sContainer.GetInstance<General>();

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
                
                sConnectionManager = sContainer.GetInstance<ConnectionManager>();

                // initialize system manager information
                sContainer.GetInstance<SystemManager>().Init(sConnectionManager);
                
                Listening listening = sContainer.GetInstance<Listening>();
                
                try
                {
                    sChannel.Trace($"Initializing server socket on port {listening.Port}...");
                    sServerSocket = new EVEServerSocket(listening.Port, sLog.CreateLogChannel("ServerSocket"));
                    sServerSocket.Listen();
                    sServerSocket.BeginAccept(acceptAsync);
                    sChannel.Debug($"Waiting for incoming connections on port {listening.Port}");
                }
                catch (Exception e)
                {
                    sChannel.Error($"Error listening on port {listening.Port}: {e.Message}");
                    throw;
                }

                long lastPickedNodeID = 0;
                
                while (true)
                {
                    // sleep for ten minutes
                    Thread.Sleep(1000 * 60 * 10);
                    
                    // check for any nodes available and pick one from the list to handle timed events
                    lock (sConnectionManager.Nodes)
                    {
                        // ignore the timed events if there's no nodes to handle them
                        if (sConnectionManager.Nodes.Count == 0)
                            continue;
                        
                        // get the first available node and request it to handle the timed events
                        // TODO: ASSIGN SOME KIND OF LOAD INDICATION TO NODES TO ENSURE THAT ONLY LOW-LOADED ONES ARE USED?
                        NodeConnection node = null;
                        try
                        {
                            // get the next node available
                            node = sConnectionManager.Nodes.First(x => x.Key > lastPickedNodeID).Value;
                        }
                        catch (InvalidOperationException)
                        {
                            // no extra node was found, so the first one has to be used
                            node = sConnectionManager.Nodes.First().Value;
                        }

                        lastPickedNodeID = node.NodeID;
                        
                        sConnectionManager.NotifyNode(lastPickedNodeID, "OnClusterTimer", new PyTuple(0));
                        sChannel.Info($"Requested node {lastPickedNodeID} to handle cluster-wide timed events");
                    }
                }
            }
            catch (Exception e)
            {
                if (sLog is null || sChannel is null)
                {
                    Console.WriteLine(e.ToString());
                }
                else
                {
                    sChannel?.Fatal(e.ToString());
                    sLog?.Flush();    
                }
            }
        }
    }
}
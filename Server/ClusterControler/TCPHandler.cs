using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Common.Packets;
using Common;

using Marshal;

namespace EVESharp.ClusterControler
{
    public static class TCPHandler
    {
        public static PyObject ProcessAuth(PyObject packet, Connection connection)
        {
            // All these packets should be a Tuple
            if (packet is PyTuple)
            {
                return HandleTuple(packet as PyTuple, connection);
            }
            else if (packet is PyObjectEx)
            {
                return HandleObject(packet as PyObjectEx, connection);
            }
            else
            {
                // Close the connection, we dont like them
                Log.Error("Client", "Got unknown packet");
                GPSTransportClosed ex = new GPSTransportClosed("None");
                connection.Send(ex.Encode());
                
                connection.EndConnection();

                return null;
            }
        }

        public static void SendLoginNotification(LoginStatus loginStatus, Connection connection)
        {
            if (loginStatus == LoginStatus.Sucess)
            {
                // We should check for a exact number of nodes here when we have the needed infraestructure
                if (ConnectionManager.NodesCount > 0)
                {
                    connection.NodeID = ConnectionManager.RandomNode;

                    AuthenticationRsp rsp = new AuthenticationRsp();

                    // String "None" marshaled
                    byte[] func_marshaled_code = new byte[] { 0x74, 0x04, 0x00, 0x00, 0x00, 0x4E, 0x6F, 0x6E, 0x65 };

                    rsp.serverChallenge = "";
                    rsp.func_marshaled_code = func_marshaled_code;
                    rsp.verification = false;
                    rsp.cluster_usercount = ConnectionManager.ClientsCount + 1; // We're not in the list yet
                    rsp.proxy_nodeid = connection.NodeID;
                    rsp.user_logonqueueposition = 1;
                    rsp.challenge_responsehash = "55087";

                    rsp.macho_version = Common.Constants.Game.machoVersion;
                    rsp.boot_version = Common.Constants.Game.version;
                    rsp.boot_build = Common.Constants.Game.build;
                    rsp.boot_codename = Common.Constants.Game.codename;
                    rsp.boot_region = Common.Constants.Game.region;

                    // Setup session
                    connection.Session.SetString("address", connection.Address);
                    connection.Session.SetString("languageID", connection.LanguageID);
                    connection.Session.SetInt("userType", Common.Constants.AccountType.User);
                    connection.Session.SetInt("userid", connection.AccountID);
                    connection.Session.SetInt("role", connection.Role);

                    // Update the connection, so it gets added to the clients correctly
                    ConnectionManager.UpdateConnection(connection);

                    connection.Send(rsp.Encode());
                }
                else
                {
                    // Pretty funny, "AutClusterStarting" maybe they mean "AuthClusterStarting"
                    GPSTransportClosed ex = new GPSTransportClosed("AutClusterStarting");
                    connection.Send(ex.Encode());

                    connection.EndConnection();
                }
            }
            else if (loginStatus == LoginStatus.Failed)
            {
                GPSTransportClosed ex = new GPSTransportClosed("LoginAuthFailed");
                connection.Send(ex.Encode());

                connection.EndConnection();
            }
        }

        private static PyObject HandleTuple(PyTuple tup, Connection connection)
        {
            int items = tup.Items.Count;

            if (items == 6)
            {
                // Only LowLeverVersionExchange
                if (connection.CheckLowLevelVersionExchange(tup) == false)
                {
                    connection.EndConnection();
                }

                if (connection.Type == ConnectionType.Node)
                {
                    // Update the list in ConnectionManager
                    ConnectionManager.UpdateConnection(connection);

                    // Send the node info
                    connection.SendNodeChangeNotification();

                    // Flag the connection as fully handled to start the correct listener
                    connection.StageEnded = true;
                }
                else if (connection.Type == ConnectionType.Client)
                {
                    // Update the list in ConnectionManager(we should do this later)
                    // ConnectionManager.UpdateConnection(connection);
                }

                return null;
            }
            else if (items == 3)
            {
                if (tup.Items[0].Type == PyObjectType.None)
                {
                    // VipKey
                    VipKeyCommand vk = new VipKeyCommand();

                    if (vk.Decode(tup) == false)
                    {
                        Log.Error("Client", "Wrong vipKey command");
                        connection.EndConnection();

                        return null;
                    }

                    return null;
                }
                else
                {
                    // Handshake sent when we are mostly in
                    HandshakeAck ack = new HandshakeAck();

                    ack.live_updates = new PyList();
                    ack.jit = connection.LanguageID;
                    ack.userid = connection.AccountID;
                    ack.maxSessionTime = new PyNone();
                    ack.userType = Common.Constants.AccountType.User;
                    ack.role = connection.Role;
                    ack.address = connection.Address;
                    ack.inDetention = new PyNone();
                    ack.client_hashes = new PyList();
                    ack.user_clientid = connection.AccountID;

                    // We have to send this just before the sessionchange
                    connection.Send(ack.Encode());

                    // Send session change
                    connection.SendSessionChange();

                    // Change the stage to ended to start the real listener
                    connection.StageEnded = true;
                }
            }
            else if (items == 2) // PlaceboRequest, QueueCheck and Login packet
            {
                if (tup.Items[0].Type == PyObjectType.None)
                {
                    QueueCheckCommand qc = new QueueCheckCommand();

                    if (qc.Decode(tup) == false)
                    {
                        Log.Error("Client", "Wrong QueueCheck command");
                        connection.EndConnection();

                        return null;
                    }

                    // Queued logins
                    connection.Send(new PyInt(LoginQueue.queue.Count + 1));
                    connection.SendLowLevelVersionExchange();

                    return null;
                }
                else if (tup.Items[0].Type == PyObjectType.String)
                {
                    if (tup.Items[0].As<PyString>().Value == "placebo")
                    {
                        // We assume it is a placebo request
                        PlaceboRequest req = new PlaceboRequest();

                        if (req.Decode(tup) == false)
                        {
                            Log.Error("Client", "Wrong placebo request");
                            connection.EndConnection();

                            return null;
                        }

                        return new PyString("OK CC");
                    }
                    else
                    {
                        // Check if the password is hashed or not and ask for plain password
                        AuthenticationReq req = new AuthenticationReq();

                        if (req.Decode(tup) == false)
                        {
                            Log.Error("Client", "Wrong login packet");

                            GPSTransportClosed ex = new GPSTransportClosed("LoginAuthFailed");

                            connection.Send(ex.Encode());
                            connection.EndConnection();

                            return null;
                        }

                        Log.Debug("Client", "Login try: " + req.user_name);

                        // The hash is in sha1, we should handle it later
                        if (req.user_password == null)
                        {
                            Log.Trace("Client", "Rejected by server; requesting plain password");
                            return new PyInt(1); // Ask for unhashed password( 1 -> Plain, 2 -> Hashed )
                        }

                        // Login request, add it to the queue and wait until we are accepted or rejected
                        LoginQueue.Enqueue(connection, req);

                        // The login queue will call send the data to the client
                        return null;
                    }
                }
            }
            else
            {
                Log.Error("Connection", "Unhandled Tuple packet with " + items + " items");
                connection.EndConnection();

                return null;
            }

            return null;
        }

        private static PyObject HandleObject(PyObjectEx dat, Connection connection)
        {
            // Only exceptions should be this type
            PyException ex = new PyException();

            if (ex.Decode(dat) == false)
            {
                Log.Error("Connection", "Unhandled PyObjectEx packet");
                return null;
            }

            Log.Error("Connection", "Got an exception packet of type: " + ex.exception_type + ". " + ex.message);
            return null;
        }
    }
}

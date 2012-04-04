using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Common;
using Marshal;
using Common.Network;
using Common.Packets;
using System.Threading;

namespace Proxy
{
    public class Client
    {
        private StreamPacketizer packetizer = null;
        private TCPSocket socket = null;
        private Thread thr = null;
        private Session session = null;
        private int nodeID = 1; // Proxy nodeID

        public Client(StreamPacketizer p, TCPSocket s)
        {
            socket = s;
            packetizer = p;
            thr = new Thread(Run);
            thr.Start();
        }

        public void AuthCorrectInfo(int accountid, int role, string languageid)
        {
            AuthenticationRsp rsp = new AuthenticationRsp();

            // String "None" marshaled
            byte[] func_marshaled_code = new byte[] { 0x74, 0x04, 0x00, 0x00, 0x00, 0x4E, 0x6F, 0x6E, 0x65 };

            rsp.serverChallenge = "";
            rsp.func_marshaled_code = func_marshaled_code;
            rsp.verification = false;
            rsp.cluster_usercount = Program.clients.Count;
            rsp.proxy_nodeid = 1; // This is the proxy ID, by the moment its the first node
            rsp.user_logonqueueposition = 1;
            rsp.challenge_responsehash = "55087";

            rsp.macho_version = Common.Constants.Game.machoVersion;
            rsp.boot_version = Common.Constants.Game.version;
            rsp.boot_build = Common.Constants.Game.build;
            rsp.boot_codename = Common.Constants.Game.codename;
            rsp.boot_region = Common.Constants.Game.region;

            // Setup session
            session.SetString("address", socket.GetAddress());
            session.SetString("languageID", languageid);
            session.SetInt("userType", Common.Constants.AccountType.User);
            session.SetInt("userid", accountid); 
            session.SetInt("role", role);

            Send(rsp.Encode());

            // Find a node
            if (NodeManager.nodes.Count == 0)
            {
                // Careful, no nodes found, close the connection
                GPSTransportClosed ex = new GPSTransportClosed("NoNodeAvailable");
                Send(ex.Encode());
                thr.Abort();
                Program.clients.Remove(this);
            }

            nodeID = NodeManager.GetRandomNode();
        }

        private void Run()
        {
            while (true)
            {
                Thread.Sleep(1);
            }
        }

        private void Send(PyObject data)
        {
            Send(Marshal.Marshal.Process(data));
        }

        private void Send(byte[] data)
        {
            socket.Send(data);
        }

        public void ChangeToNode(int newNodeID)
        {
            nodeID = newNodeID;
            SendSessionChange();
        }

        public void SendSessionChange()
        {
            SessionChangeNotification scn = new SessionChangeNotification();
            scn.changes = session.EncodeChanges();

            if (scn.changes.Dictionary.Count == 0)
            {
                // Nothing to do
                return;
            }

            // Add our current nodeID
            scn.nodesOfInterest.Items.Add(new PyInt(nodeID));

            // Add the proxy ID
            scn.nodesOfInterest.Items.Add(new PyInt(1));

            // Add all the nodeIDs
            foreach (int node in NodeManager.nodes.Keys)
            {
                if(node != nodeID)
                    scn.nodesOfInterest.Items.Add(new PyInt(node));
            }

            PyPacket p = new PyPacket();

            p.type_string = "macho.SessionChangeNotification";
            p.type = Macho.MachoNetMsg_Type.SESSIONCHANGENOTIFICATION;

            p.source.type = PyAddress.AddrType.Node;
            p.source.typeID = (ulong)nodeID;
            p.source.callID = 0;

            p.dest.type = PyAddress.AddrType.Client;
            p.dest.typeID = (ulong)GetAccountID();
            p.dest.callID = 0;

            p.userID = (uint)GetAccountID();

            p.payload = scn.Encode().As<PyTuple>();

            p.named_payload = new PyDict();
            p.named_payload.Set("channel", new PyString("sessionchange"));
        }

        public string GetLanguageID()
        {
            return session.GetCurrentString("languageID");
        }

        public int GetAccountID()
        {
            return session.GetCurrentInt("userid");
        }

        public int GetAccountRole()
        {
            return session.GetCurrentInt("role");
        }

        public string GetAddress()
        {
            return session.GetCurrentString("address");
        }
    }
}

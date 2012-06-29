using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Marshal;
using Common;

namespace EVESharp
{
    class Client
    {
        private Session session = new Session();

        public void UpdateSession(PyPacket from)
        {
            Log.Debug("Client", "Updating session for client");

            // We should add a Decode method to SessionChangeNotification...
            PyTuple payload = from.payload;

            PyDict changes = payload[0].As<PyTuple>()[1].As<PyTuple>()[0].As<PyDict>();

            // Update our local session
            foreach(PyString key in changes.Dictionary.Keys)
            {
                session.Set(key.Value, changes[key.Value].As<PyTuple>()[1]);
            }
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

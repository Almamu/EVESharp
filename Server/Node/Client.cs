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

            PyDict changes = payload[0].As<PyTuple>()[1].As<PyDict>();

            // Update our local session
            foreach(PyString key in changes.Dictionary.Keys)
            {
                session.Set(key.Value, changes[key.Value].As<PyTuple>()[1]);
            }
        }

        public string LanguageID
        {
            get
            {
                return session.GetCurrentString("languageID");
            }

            set
            {
                session.SetString("languageID", value);
            }
        }

        public int AccountID
        {
            get
            {
                return session.GetCurrentInt("userid");
            }

            set
            {

            }
        }

        public int Role
        {
            get
            {
                return session.GetCurrentInt("role");
            }

            set
            {

            }
        }

        public string Address
        {
            get
            {
                return session.GetCurrentString("address");
            }

            set
            {

            }
        }
    }
}

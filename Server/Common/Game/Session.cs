using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Marshal;

namespace Common.Game
{
    // TODO: ADD BETTER WAYS TO WORK WITH THE SESSION
    public class Session
    {
        private Dictionary<string, PyTuple> SessionData = new Dictionary<string, PyTuple>();
        
        private void Set(string key, PyObject newValue)
        {
            if (SessionData.ContainsKey(key) == false)
            {
                PyTuple var = new PyTuple();

                var.Items.Add(new PyNone());
                var.Items.Add(newValue);

                SessionData.Add(key, var);
            }
            else
            {
                PyTuple tmp = SessionData[key] as PyTuple;

                tmp.Items[0] = tmp.Items[1];
                tmp.Items[1] = newValue;

                SessionData[key] = tmp;
            }
        }

        private PyTuple Get(string key)
        {
            if (SessionData.ContainsKey(key) == false)
            {
                PyTuple tuple = new PyTuple();

                // add two items to the list to simulate an actual tuple
                tuple.Items.Add(new PyNone ());
                tuple.Items.Add(new PyNone());
                
                return tuple;
            }
            else
            {
                return SessionData[key] as PyTuple;
            }
        }

        private PyObject GetCurrent(string key)
        {
            PyTuple values = this.Get(key);

            return values.Items[1];
        }

        public void SetString(string name, string value)
        {
            this.Set(name, new PyString(value));
        }

        public void SetInt(string name, int value)
        {
            this.Set(name, new PyInt(value));
        }

        public void SetLong(string name, long value)
        {
            this.Set(name, new PyLongLong(value));
        }

        public string GetCurrentString(string name)
        {
            PyObject res = this.GetCurrent(name);

            if (res.Type != PyObjectType.String)
            {
                return "";
            }

            return res.As<PyString>().Value;
        }

        public int GetCurrentInt(string name)
        {
            PyObject res = this.GetCurrent(name);

            if((res.Type != PyObjectType.Long) || (res.Type != PyObjectType.IntegerVar))
            {
                return 0;
            }

            return res.As<PyInt>().Value;
        }

        public long GetCurrentLong(string name)
        {
            PyObject res = this.GetCurrent(name);

            if (res.Type != PyObjectType.LongLong)
            {
                return 0;
            }

            return res.IntValue;
        }

        public bool KeyExists(string name)
        {
            return SessionData.ContainsKey(name);
        }
        
        public PyDict EncodeChanges()
        {
            PyDict result = new PyDict();
            PyDict tmp = new PyDict();

            // Iterate through the session data
            foreach (KeyValuePair<string, PyTuple> entry in SessionData)
            {
                PyObject last = entry.Value.Items[0];
                PyObject current = entry.Value.Items[1];

                // Check if they have the same type and value
                if (last != current)
                {
                    // Add the change to the dict
                    PyTuple change = new PyTuple();

                    change.Items.Add(last);
                    change.Items.Add(current);
                    // build the change object
                    result.Set(entry.Key, change);
                    // update the old value with the one synced to the client
                    entry.Value.Items[0] = entry.Value.Items[1];
                }
            }

            return result;
        }
    }
}

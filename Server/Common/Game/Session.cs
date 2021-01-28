using System.Collections.Generic;
using PythonTypes.Types.Primitives;

namespace Common.Game
{
    public class Session
    {
        private PyDictionary mSession;
        public bool IsDirty { get; private set; }
        
        public PyDataType this[string key]
        {
            get => this.GetCurrent(key);
            set => this.SetCurrent(key, value);
        }

        public Session()
        {
            this.mSession = new PyDictionary();
            this.IsDirty = false;
        }

        public void SetCurrent(string key, PyDataType value)
        {
            this.IsDirty = true;
            
            if (this.mSession.ContainsKey(key) == false)
            {
                PyTuple var = new PyTuple(2);

                var[0] = new PyNone();
                var[1] = value;

                this.mSession[key] = var;
            }
            else
            {
                PyTuple tmp = this.mSession[key] as PyTuple;

                tmp[0] = tmp[1];
                tmp[1] = value;

                this.mSession[key] = tmp;
            }
        }

        public PyDataType GetCurrent(string key)
        {
            if (this.mSession.ContainsKey(key) == false)
                return new PyNone();

            PyTuple pair = this.mSession[key] as PyTuple;

            return pair[1];
        }

        public PyDataType GetPrevious(string key)
        {
            if (this.mSession.ContainsKey(key) == false)
                return new PyNone();

            PyTuple pair = this.mSession[key] as PyTuple;

            return pair[0];
        }

        public bool ContainsKey(string key)
        {
            return this.mSession.ContainsKey(key);
        }

        public PyDictionary GenerateSessionChange()
        {
            PyDictionary result = new PyDictionary();

            // iterate through the session data
            foreach (KeyValuePair<PyDataType, PyDataType> pair in this.mSession)
            {
                PyTuple value = pair.Value as PyTuple;

                PyDataType last = value[0];
                PyDataType current = value[1];

                // encode only data that has changed
                if (last != current)
                {
                    // create a new tuple to send as the session-change notification
                    result[pair.Key] = new PyTuple(new PyDataType[] { last, current });

                    // update the data in the session to reflect no change
                    value[0] = value[1];
                }
            }

            this.IsDirty = false;
            
            return result;
        }

        public void LoadChanges(PyDictionary changes)
        {
            // parse the encoded changes and update the current values
            foreach (KeyValuePair<PyDataType, PyDataType> pair in changes)
                this[pair.Key as PyString] = (pair.Value as PyTuple)[1];

            // session is not dirty if we're updating from the changes of another node
            this.IsDirty = false;
        }
    }
}
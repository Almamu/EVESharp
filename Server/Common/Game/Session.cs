using System.Collections.Generic;
using System.Threading;
using PythonTypes.Types.Collections;
using PythonTypes.Types.Primitives;

namespace Common.Game
{
    public class Session
    {
        private readonly PyDictionary<PyString,PyTuple> mSession;
        public bool IsDirty { get; private set; }
        
        public PyDataType this[string key]
        {
            get => this.GetCurrent(key);
            set => this.SetCurrent(key, value);
        }

        public Session()
        {
            this.mSession = new PyDictionary<PyString,PyTuple>();
            this.IsDirty = false;
        }

        public Session(PyDictionary<PyString,PyTuple> from)
        {
            this.mSession = from;
            this.IsDirty = false;
        }

        public void SetCurrent(string key, PyDataType value)
        {
            lock (this.mSession)
            {
                this.IsDirty = true;
            
                if (this.mSession.ContainsKey(key) == false)
                {
                    this.mSession[key] = new PyTuple(2)
                    {
                        [0] = null,
                        [1] = value
                    };
                }
                else
                {
                    PyTuple tmp = this.mSession[key];

                    tmp[0] = tmp[1];
                    tmp[1] = value;

                    this.mSession[key] = tmp;
                }
            }
        }

        public PyDataType GetCurrent(string key)
        {
            lock (this.mSession)
            {
                if (this.mSession.TryGetValue(key, out PyTuple pair) == false)
                    return null;

                return pair[1];
            }
        }

        public PyDataType GetPrevious(string key)
        {
            lock (this.mSession)
            {
                if (this.mSession.TryGetValue(key, out PyTuple pair) == false)
                    return null;

                return pair[0];
            }
        }

        public bool ContainsKey(string key)
        {
            lock(this.mSession)
                return this.mSession.ContainsKey(key);
        }

        public PyDictionary GenerateSessionChange()
        {
            lock (this.mSession)
            {
                PyDictionary result = new PyDictionary();

                // iterate through the session data
                foreach ((PyString key, PyTuple value) in this.mSession)
                {
                    PyDataType last = value[0];
                    PyDataType current = value[1];

                    // encode only data that has changed
                    if (last != current)
                    {
                        // create a new tuple to send as the session-change notification
                        result[key] = new PyTuple(2) { [0] = last, [1] = current };

                        // update the data in the session to reflect no change
                        value[0] = value[1];
                    }
                }

                this.IsDirty = false;
            
                return result;
            }
        }

        public void LoadChanges(PyDictionary changes)
        {
            // parse the encoded changes and update the current values
            foreach ((PyString key, PyTuple value) in changes.GetEnumerable<PyString, PyTuple>())
                this[key] = value[1];

            // session is not dirty if we're updating from the changes of another node
            this.IsDirty = false;
        }
    }
}
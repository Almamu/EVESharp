using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PythonTypes;
using PythonTypes.Types.Primitives;

namespace Common.Game
{
    public class Session : PyDictionary
    {
        public new PyDataType this[string key]
        {
            get
            {
                if (this.ContainsKey(key) == false)
                    return new PyNone();

                PyTuple data = base[key] as PyTuple;

                return data[1];
            }

            set
            {
                // ensure there is a key stored if the element is new
                if(this.ContainsKey(key) == false)
                    base[key] = new PyTuple(new PyDataType [] { new PyNone(), new PyNone() });

                PyTuple entry = base[key] as PyTuple;

                entry[0] = entry[1];
                entry[1] = value;

                base[key] = entry;
            }
        }

        public PyDictionary GenerateSessionChange()
        {
            PyDictionary result = new PyDictionary();

            foreach (KeyValuePair<string, PyDataType> pair in this)
            {
                PyTuple value = pair.Value as PyTuple;
                
                result[pair.Key] = new PyTuple(new []{ value[0], value[1] });

                value[0] = value[1];
            }

            return result;
        }
    }
}

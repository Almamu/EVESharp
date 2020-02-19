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

using System.Collections.Generic;
using PythonTypes.Types.Primitives;

namespace Node
{
    public class Session
    {
        private PyDictionary session;

        public Session()
        {
            session = new PyDictionary();
        }

        private void _Set(string key, PyDataType val)
        {
            if (session.ContainsKey(key) == false)
            {
                PyTuple var = new PyTuple(2);

                var[0] = new PyNone();
                var[1] = val;

                session[key] = var;
            }
            else
            {
                PyTuple tmp = session[key] as PyTuple;

                tmp[0] = tmp[1];
                tmp[1] = val;

                session[key] = tmp;
            }
        }

        private PyTuple _Get(string key)
        {
            if (session.ContainsKey(key))
                return session[key] as PyTuple;
            else
                return new PyTuple(0);
        }

        private PyDataType _GetCurrent(string key)
        {
            PyTuple tmp = _Get(key);

            if (tmp.Count == 0)
                return new PyNone();

            return tmp[1];
        }

        public void Set(string name, PyDataType value)
        {
            _Set(name, value);
        }

        public PyDataType GetCurrent(string name)
        {
            return _GetCurrent(name);
        }

        public PyDictionary EncodeChanges()
        {
            PyDictionary res = new PyDictionary();
            PyDictionary tmp = new PyDictionary();

            // Iterate through the session data
            foreach (KeyValuePair<string, PyDataType> s in session)
            {
                PyTuple value = s.Value as PyTuple;

                PyDataType last = value[0];
                PyDataType current = value[1];

                // Check if they have the same type and value
                if (last != current)
                {
                    PyTuple change = new PyTuple(2);

                    change[0] = last;
                    change[1] = current;

                    res[s.Key] = change;

                    // Update the session with the new last value
                    PyTuple update = new PyTuple(2);

                    update[0] = current;
                    update[1] = current;

                    // We cant modify the session here, just store it on something temporal
                    tmp[s.Key] = update;
                }
            }

            // Send back the session from temporal to the final storage
            session = tmp;

            return res;
        }
    }
}
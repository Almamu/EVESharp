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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Marshal;

namespace Node
{
    public class Session
    {
        private PyDict session;

        public Session()
        {
            session = new PyDict();
        }

        private void _Set(PyObject key, PyObject val)
        {
            if (session.Dictionary.ContainsKey(key) == false)
            {
                PyTuple var = new PyTuple();

                var.Items.Add(new PyNone());
                var.Items.Add(val);

                session.Dictionary.Add(key, var);
            }
            else
            {
                PyTuple tmp = session.Dictionary[key].As<PyTuple>();

                tmp.Items[0] = tmp.Items[1];
                tmp.Items[1] = val;

                session.Dictionary[key] = tmp;
            }
        }

        private PyTuple _Get(PyObject key)
        {
            if (session.Dictionary.ContainsKey(key))
            {
                return session[key].As<PyTuple>();
            }
            else
            {
                return new PyTuple();
            }
        }

        private PyObject _GetCurrent(PyObject key)
        {
            PyTuple tmp = _Get(key);

            if (tmp.Items.Count == 0)
            {
                return new PyNone();
            }

            return tmp.Items[1];
        }

        public void Set(string name, PyObject value)
        {
            _Set(new PyString(name), value);
        }


        public void SetString(string name, string value)
        {
            _Set(new PyString(name), new PyString(value));
        }

        public void SetInt(string name, int value)
        {
            _Set(new PyString(name), new PyInt(value));
        }

        public PyObject GetCurrent(string name)
        {
            return _Get(new PyString(name));
        }

        public string GetCurrentString(string name)
        {
            PyObject res = _GetCurrent(new PyString(name));

            if (res.Type != PyObjectType.String)
            {
                return "";
            }

            return res.As<PyString>().Value;
        }

        public int GetCurrentInt(string name)
        {
            PyObject res = _GetCurrent(new PyString(name));

            if((res.Type != PyObjectType.Long) || (res.Type != PyObjectType.IntegerVar))
            {
                return 0;
            }

            return res.As<PyInt>().Value;
        }

        public PyDict EncodeChanges()
        {
            PyDict res = new PyDict();
            PyDict tmp = new PyDict();

            // Iterate through the session data
            foreach (PyString s in session.Dictionary.Keys)
            {
                PyTuple value = session[s].As<PyTuple>();

                PyObject last = value.Items[0];
                PyObject current = value.Items[1];

                // Check if they have the same type and value
                if (last != current)
                {
                    // Add the change to the dict
                    PyTuple change = new PyTuple();

                    change.Items.Add(last);
                    change.Items.Add(current);

                    res.Set(s.Value, change);

                    // Update the session with the new last value
                    PyTuple update = new PyTuple();

                    update.Items.Add(current);
                    update.Items.Add(current);

                    // We cant modify the session here, just store it on something temporal
                    tmp[s] = update;
                }
            }

            // Send back the session from temporal to the final storage
            session = tmp;

            return res;
        }
    }
}

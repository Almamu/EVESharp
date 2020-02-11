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
using Common;
using Common.Logging;

namespace Node
{
    public class Client
    {
        private Session session = new Session();

        public void UpdateSession(PyPacket from)
        {
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

/*
    ------------------------------------------------------------------------------------
    LICENSE:
    ------------------------------------------------------------------------------------
    This file is part of EVE#: The EVE Online Server Emulator
    Copyright 2021 - EVE# Team
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
using Common.Logging;
using Common.Services;
using Node.Network;
using PythonTypes.Types.Collections;
using PythonTypes.Types.Primitives;

namespace Node.Services.Network
{
    public class alert : Service
    {
        private Channel Log { get; }

        public alert(Logger logger)
        {
            this.Log = logger.CreateLogChannel("alert");
        }

        public PyTuple BeanCount(PyInteger stackID, CallInformation call)
        {
            return new PyTuple(2)
            {
                [0] = null,
                [1] = 0
            };
        }

        public PyDataType SendClientStackTraceAlert(PyTuple stackInfo, PyString stackTrace, PyString type, PyDataType extra = null, CallInformation call = null)
        {
            Log.Fatal(
                "Received the following client's stack trace:" + Environment.NewLine +
                $"------------------ {type.Value} ------------------" + Environment.NewLine +
                $"{(stackInfo[1] as PyString).Value}" + Environment.NewLine +
                stackTrace.Value
            );
            
            // the client should receive anything to know that the stack trace arrived to the server
            return null;
        }

        public PyDataType BeanDelivery(PyDictionary beanCounts, CallInformation call)
        {
            // I'm not joking, send me the stack trace NOW!!!
            // :P
            return null;
        }
    }
}
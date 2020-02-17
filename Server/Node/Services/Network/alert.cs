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
using PythonTypes;
using Common;
using Common.Services;
using System.IO;
using Common.Logging;
using PythonTypes.Types.Primitives;

namespace Node.Services.Network
{
    public class alert : Service
    {
        private Channel Log { get; set; }
        public alert(Logger logger)
            : base("alert")
        {
            this.Log = logger.CreateLogChannel("alert");
        }

        public PyDataType BeanCount(PyTuple args, object client)
        {
            PyTuple res = new PyTuple(2);
            
            res[0] = new PyNone();
            res[1] = new PyInteger(0);
            
            return res;
        }

        public PyDataType SendClientStackTraceAlert(PyTuple args, object client)
        {
            Log.Fatal(
                "Received the following client's stack trace:\n" + 
                $"------------------ {args[2] as PyString} ------------------\n" +
                $"{(args[0] as PyTuple)[1] as PyString}\n" +
                (args[1] as PyString)
            );
            // The client should receive anything to know that the stack trace arrived to the server
            return new PyNone();
        }

        public PyDataType BeanDelivery(PyTuple args, object client)
        {
            // I'm not joking, send me the stack trace NOW!!!
            // :P
            return new PyNone();
        }
    }
}

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

using EVESharp.EVE.Exceptions;
using EVESharp.EVE.Packets.Exceptions;
using EVESharp.EVE.Services;
using EVESharp.Node.Cache;
using EVESharp.PythonTypes.Types.Collections;
using EVESharp.PythonTypes.Types.Primitives;
using Serilog;

namespace EVESharp.Node.Services.CacheSvc;

public class objectCaching : Service
{
    public override AccessLevel  AccessLevel  => AccessLevel.None;
    private         ILogger      Log          { get; }
    private         CacheStorage CacheStorage { get; }

    public objectCaching (CacheStorage cacheStorage, ILogger logger)
    {
        Log          = logger;
        CacheStorage = cacheStorage;
    }

    public PyDataType GetCachableObject (CallInformation call, PyInteger shared, PyTuple objectID, PyTuple objectVersion, PyInteger nodeID)
    {
        // TODO: CHECK CACHEOK EXCEPTION ON CLIENT
        Log.Debug ("Received cache request for a tuple objectID");

        if (objectID.Count != 3 || objectID [2] is PyTuple == false)
            throw new CustomError ("Requesting cache with an unknown objectID");

        PyTuple callInformation = objectID [2] as PyTuple;

        if (callInformation.Count != 2 || callInformation [0] is PyString == false ||
            callInformation [1] is PyString == false)
            throw new CustomError ("Requesting cache with an unknown objectID");

        string service = callInformation [0] as PyString;
        string method  = callInformation [1] as PyString;

        return CacheStorage.Get (service, method);
    }

    public PyDataType GetCachableObject (CallInformation call, PyInteger shared, PyString objectID, PyTuple objectVersion, PyInteger nodeID)
    {
        // TODO: CHECK CACHEOK EXCEPTION ON CLIENT
        Log.Debug ($"Received cache request for {objectID.Value}");

        return CacheStorage.Get (objectID);
    }
}
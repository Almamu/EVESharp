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

using EVESharp.EVE.Packets.Exceptions;
using EVESharp.EVE.Services;
using EVESharp.Node.Network;
using EVESharp.PythonTypes.Types.Collections;
using EVESharp.PythonTypes.Types.Primitives;
using Serilog;

namespace EVESharp.Node.Services.CacheSvc;

public class objectCaching : Service
{
    public override AccessLevel  AccessLevel  => AccessLevel.None;
    private         ILogger      Log          { get; }
    private         CacheStorage CacheStorage { get; }

    public objectCaching(CacheStorage cacheStorage, ILogger logger)
    {
        this.Log          = logger;
        this.CacheStorage = cacheStorage;
    }
        
    public PyDataType GetCachableObject(PyInteger shared, PyTuple objectID, PyTuple objectVersion, PyInteger nodeID, CallInformation call)
    {
        // TODO: CHECK CACHEOK EXCEPTION ON CLIENT
        Log.Debug($"Received cache request for a tuple objectID");
            
        if (objectID.Count != 3 || objectID [2] is PyTuple == false)
            throw new CustomError("Requesting cache with an unknown objectID");

        PyTuple callInformation = objectID[2] as PyTuple;

        if (callInformation.Count != 2 || callInformation[0] is PyString == false ||
            callInformation[1] is PyString == false)
            throw new CustomError("Requesting cache with an unknown objectID");

        string service = callInformation[0] as PyString;
        string method  = callInformation[1] as PyString;
            
        return this.CacheStorage.Get(service, method);
    }

    public PyDataType GetCachableObject(PyInteger shared, PyString objectID, PyTuple objectVersion, PyInteger nodeID, CallInformation call)
    {
        // TODO: CHECK CACHEOK EXCEPTION ON CLIENT
        Log.Debug($"Received cache request for {objectID.Value}");

        return this.CacheStorage.Get(objectID);
    }
}
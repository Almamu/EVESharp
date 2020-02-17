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
using Common;
using Common.Logging;
using PythonTypes;
using Common.Services;
using Common.Packets;
using PythonTypes.Types.Complex;
using PythonTypes.Types.Primitives;

namespace Node.Services.CacheSvc
{
    public class objectCaching : Service
    {
        private Channel Log { get; set; }
        private CacheStorage mCacheStorage = null;
        
        public objectCaching(CacheStorage cacheStorage, Logger logger)
            : base("objectCaching")
        {
            this.Log = logger.CreateLogChannel("objectCaching");
            this.mCacheStorage = cacheStorage;
        }

        public PyDataType GetCachableObject(PyTuple args, object client)
        {
            PyCacheHint cache = args;
            
            if(cache.objectID is PyString == false)
            {
                Log.Error("Unknown objectID on cache request");
                return null;
            }

            string objectID = cache.objectID as PyString;

            Log.Debug($"Received cache request for {objectID}");
            
            return this.mCacheStorage.Get(objectID);
        }
    }
}

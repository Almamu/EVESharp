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
using System.Reflection;
using EVESharp.EVE.Services;
using EVESharp.Node.Network;
using EVESharp.PythonTypes.Types.Collections;
using EVESharp.PythonTypes.Types.Primitives;
using ServiceManager = EVESharp.Node.Network.ServiceManager;

namespace EVESharp.Node.Services.Network
{
    public class machoNet : Service
    {
        public override AccessLevel AccessLevel => AccessLevel.None;
        private CacheStorage CacheStorage { get; }
        
        public machoNet(CacheStorage cacheStorage)
        {
            this.CacheStorage = cacheStorage;
        }

        public PyTuple GetInitVals(CallInformation call)
        {
            if (this.CacheStorage.Exists("machoNet.serviceInfo") == false)
            {
                PyDictionary dict = new PyDictionary();
                
                // indicate the required access levels for the service to be callable
                foreach (PropertyInfo property in call.ServiceManager.GetType().GetProperties(BindingFlags.Public))
                {
                    object value = property.GetValue(call.ServiceManager);
                    
                    // ignore things that are not services
                    if (value is not Service)
                        continue;

                    Service service = value as Service;

                    dict[service.Name] = service.AccessLevel switch
                    {
                        AccessLevel.Location => "location",
                        AccessLevel.LocationPreferred => "locationPreferred",
                        AccessLevel.Station => "station",
                        AccessLevel.SolarSystem => "solarsystem",
                        _ => null,
                    };
                }
                
                this.CacheStorage.Store("machoNet.serviceInfo", dict, DateTime.UtcNow.ToFileTimeUtc());
            }

            return new PyTuple(2)
            {
                [0] = this.CacheStorage.GetHint("machoNet.serviceInfo"),
                [1] = this.CacheStorage.GetHints(CacheStorage.LoginCacheTable)
            };
        }

        public PyInteger GetTime(CallInformation call)
        {
            return DateTime.UtcNow.ToFileTimeUtc();
        }
    }
}
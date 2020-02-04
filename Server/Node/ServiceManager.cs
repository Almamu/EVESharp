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
using Common.Database;
using Common.Services;
using Node.Services.CacheSvc;
using Node.Services.Characters;
using Node.Services.Network;

namespace Node
{
    public class ServiceManager : Common.Services.ServiceManager
    {
        private CacheStorage mCacheStorage = null;
        private DatabaseConnection mDatabaseConnection = null;
        private objectCaching mObjectCachingSvc = null;
        private machoNet mMachoNetSvc = null;
        private alert mAlertSvc = null;
        private authentication mAuthenticationSvc = null;
        private character mCharacterSvc = null;

        public Service objectCaching()
        {
            return this.mObjectCachingSvc;
        }

        public Service machoNet()
        {
            return this.mMachoNetSvc;
        }

        public Service alert()
        {
            return this.mAlertSvc;
        }

        public Service authentication()
        {
            return this.mAuthenticationSvc;
        }

        public Service character()
        {
            return this.mCharacterSvc;
        }
        
        public ServiceManager(DatabaseConnection db, CacheStorage storage, Configuration.General configuration)
        {
            this.mDatabaseConnection = db;
            this.mCacheStorage = storage;
            
            // initialize services
            this.mMachoNetSvc = new machoNet(this.mCacheStorage);
            this.mAlertSvc = new alert();
            this.mObjectCachingSvc = new objectCaching(this.mCacheStorage);
            this.mAuthenticationSvc = new authentication(configuration.Authentication);
            this.mCharacterSvc = new character(db);
        }
    }
}

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
using Node.Database;

namespace Node.Inventory.SystemEntities
{
    public struct SolarSystemInfo
    {
        public int regionID;
        public int constellationID;
        public double x, y, z, xMin, yMin, zMin, xMax, yMax, zMax;
        public double luminosity;
        public bool border;
        public bool fringe;
        public bool corridor;
        public bool hub;
        public bool international;
        public bool regional;
        public bool constellation;
        public double security;
        public int factionID;
        public double radius;
        public int sunTypeID;
        public string securityClass;
    }

    public class SolarSystemLoadException : Exception
    {
        public SolarSystemLoadException()
            : base("Cannot load solar system from database")
        {

        }
    }
    
    public class SolarSystem : Inventory
    {
        public SolarSystem(Entity from, SolarSystemInfo info) : base(from)
        {
            if (typeID != 5)
            {
                throw new Exception("Trying to load a non-solar system item like one");
            }

            solarSystemInfo = info;
        }

        public SolarSystemInfo solarSystemInfo { private set; get; }
    }
}

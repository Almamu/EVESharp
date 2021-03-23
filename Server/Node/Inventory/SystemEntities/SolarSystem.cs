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
using Node.Inventory.Items;
using PythonTypes.Types.Collections;
using PythonTypes.Types.Database;
using PythonTypes.Types.Primitives;

namespace Node.Inventory.SystemEntities
{
    public class SolarSystem : ItemInventory
    {
        public SolarSystem(ItemEntity from, int regionId, int constellationId, double mapX, double mapY, double mapZ,
            double mapXMin, double mapYMin, double mapZMin, double mapXMax, double mapYMax, double mapZMax, double luminosity,
            bool border, bool fringe, bool corridor, bool hub, bool international, bool regional, bool constellation,
            double security, int? factionId, double radius, int sunTypeId, string securityClass) : base(from)
        {
            this.mRegionId = regionId;
            this.mConstellationId = constellationId;
            this.mMapX = mapX;
            this.mMapY = mapY;
            this.mMapZ = mapZ;
            this.mMapXMin = mapXMin;
            this.mMapYMin = mapYMin;
            this.mMapZMin = mapZMin;
            this.mMapXMax = mapXMax;
            this.mMapYMax = mapYMax;
            this.mMapZMax = mapZMax;
            this.mLuminosity = luminosity;
            this.mBorder = border;
            this.mFringe = fringe;
            this.mCorridor = corridor;
            this.mHub = hub;
            this.mInternational = international;
            this.mRegional = regional;
            this.mConstellation = constellation;
            this.mSecurity = security;
            this.mFactionId = factionId;
            this.mRadius = radius;
            this.mSunTypeId = sunTypeId;
            this.mSecurityClass = securityClass;
            this.mBelongsToUs = false;
        }

        private int mRegionId;
        private int mConstellationId;
        private double mMapX, mMapY, mMapZ, mMapXMin, mMapYMin, mMapZMin, mMapXMax, mMapYMax, mMapZMax;
        private double mLuminosity;
        private bool mBorder;
        private bool mFringe;
        private bool mCorridor;
        private bool mHub;
        private bool mInternational;
        private bool mRegional;
        private bool mConstellation;
        private double mSecurity;
        private int? mFactionId;
        private double mRadius;
        private int mSunTypeId;
        private string mSecurityClass;
        private bool mBelongsToUs;

        public int RegionID
        {
            get => this.mRegionId;
        }

        public int ConstellationID
        {
            get => this.mConstellationId;
        }

        public double MapX
        {
            get => mMapX;
        }

        public double MapY
        {
            get => mMapY;
        }

        public double MapZ
        {
            get => mMapZ;
        }

        public double MapXMin
        {
            get => mMapXMin;
        }

        public double MapYMin
        {
            get => mMapYMin;
        }

        public double MapZMin
        {
            get => mMapZMin;
        }

        public double MapXMax
        {
            get => mMapXMax;
        }

        public double MapYMax
        {
            get => mMapYMax;
        }

        public double MapZMax
        {
            get => mMapZMax;
        }

        public double Luminosity
        {
            get => mLuminosity;
        }

        public bool Border
        {
            get => mBorder;
        }

        public bool Fringe
        {
            get => mFringe;
        }

        public bool Corridor
        {
            get => mCorridor;
        }

        public bool Hub
        {
            get => mHub;
        }

        public bool International
        {
            get => mInternational;
        }

        public bool Regional
        {
            get => mRegional;
        }

        public bool Constellation
        {
            get => mConstellation;
        }

        public double Security
        {
            get => mSecurity;
        }

        public int? FactionID
        {
            get => mFactionId;
        }

        public double Radius
        {
            get => mRadius;
        }

        public int SunTypeID
        {
            get => mSunTypeId;
        }

        public string SecurityClass
        {
            get => mSecurityClass;
        }

        public bool BelongsToUs
        {
            get => this.mBelongsToUs;
            set => this.mBelongsToUs = value;
        }
        
        protected override void LoadContents(ItemFlags ignoreFlags = ItemFlags.None)
        {
            throw new NotImplementedException();
        }

        protected override void SaveToDB()
        {
            // solar systems cannot be updated
            throw new NotImplementedException();
        }

        public override void Destroy()
        {
            throw new NotImplementedException("Stations cannot be destroyed as they're regarded as static data!");
        }

        public PyDataType GetSolarSystemInfo()
        {
            // TODO: CHECK WHERE WE CAN FETCH allianceID, sovereigntyLevel and constellationSovereignty
            // TODO: AS THESE SEEM TO BE DYNAMIC VALUES
            return new Row(
                new PyList(14)
                {
                    [0]  = "solarSystemID",
                    [1]  = "solarSystemName",
                    [2]  = "x",
                    [3]  = "y",
                    [4]  = "z",
                    [5]  = "radius",
                    [6]  = "security",
                    [7]  = "constellationID",
                    [8]  = "factionID",
                    [9]  = "sunTypeID",
                    [10] = "regionID",
                    [11] = "allianceID",
                    [12] = "sovereigntyLevel",
                    [13] = "constellationSovereignty"
                },
                new PyList(14)
                {
                    [0]  = this.ID,
                    [1]  = this.Name,
                    [2]  = this.X,
                    [3]  = this.Y,
                    [4]  = this.Z,
                    [5]  = this.Radius,
                    [6]  = this.Security,
                    [7]  = this.ConstellationID,
                    [8]  = this.FactionID,
                    [9]  = this.SunTypeID,
                    [10] = this.RegionID,
                    [11] = null,
                    [12] = 0,
                    [13] = 0
                }
            );
        }
    }
}
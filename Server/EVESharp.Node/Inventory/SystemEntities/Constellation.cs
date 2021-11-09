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
using EVESharp.Node.Inventory.Items;
using EVESharp.Node.StaticData.Inventory;

namespace EVESharp.Node.Inventory.SystemEntities
{
    public class Constellation : ItemInventory
    {
        public Constellation(ItemEntity from, int regionId, double x, double y, double z,
            double xMin, double yMin, double zMin, double xMax, double yMax, double zMax, int? factionId,
            double radius) : base(from)
        {
            this.mRegionId = regionId;
            this.mX = x;
            this.mY = y;
            this.mZ = z;
            this.mXMin = xMin;
            this.mYMin = yMin;
            this.mZMin = zMin;
            this.mXMax = xMax;
            this.mYMax = yMax;
            this.mZMax = zMax;
            this.mFactionId = factionId;
            this.mRadius = radius;
        }

        private int mRegionId;
        private double mX, mY, mZ, mXMin, mYMin, mZMin, mXMax, mYMax, mZMax;
        private int? mFactionId;
        private double mRadius;

        public int RegionID
        {
            get => this.mRegionId;
        }

        public new double X
        {
            get => mX;
        }

        public new double Y
        {
            get => mY;
        }

        public new double Z
        {
            get => mZ;
        }

        public double XMin
        {
            get => mXMin;
        }

        public double YMin
        {
            get => mYMin;
        }

        public double ZMin
        {
            get => mZMin;
        }

        public double XMax
        {
            get => mXMax;
        }

        public double YMax
        {
            get => mYMax;
        }

        public double ZMax
        {
            get => mZMax;
        }

        public int? FactionID
        {
            get => mFactionId;
        }

        public double Radius
        {
            get => mRadius;
        }

        protected override void LoadContents(Flags ignoreFlags = Flags.None)
        {
            throw new NotImplementedException();
        }

        protected override void SaveToDB()
        {
            // constellations cannot be updated
            throw new NotImplementedException();
        }

        public override void Destroy()
        {
            throw new NotImplementedException("Stations cannot be destroyed as they're regarded as static data!");
        }
    }
}
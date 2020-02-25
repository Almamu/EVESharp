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

using System.Diagnostics;
using Common.Database;
using Node.Database;

namespace Node.Inventory
{
    public class Entity : DatabaseEntity
    {
        public ItemFactory mItemFactory = null;

        private int mID;
        private string mName;
        private ItemType mType;
        private int mOwnerID;
        private int mLocationID;
        private int mFlag;
        private bool mContraband;
        private bool mSingleton;
        private int mQuantity; // TODO: DEPRECATE THIS AND USE QUANTITY ATTRIBUTE
        private double mX;
        private double mY;
        private double mZ;
        private string mCustomInfo;
        private AttributeList mAttributes;

        public int ID => mID;
        public ItemType Type => mType;
        public AttributeList Attributes => mAttributes;

        public string Name
        {
            get => mName;
            set 
            {
                mName = value;
                this.Dirty = true;
            }
        }

        public int OwnerID
        {
            get => mOwnerID;
            set
            {
                this.mOwnerID = value;
                this.Dirty = true;
            }
        }

        public int LocationID
        {
            get => mLocationID;
            set
            {
                this.mLocationID = value;
                this.Dirty = true;
            }
        }

        public int Flag
        {
            get => mFlag;
            set
            {
                this.mFlag = value;
                this.Dirty = true;
            }
        }

        public bool Contraband
        {
            get => mContraband;
            set
            {
                this.mContraband = value;
                this.Dirty = true;
            }
        }

        public bool Singleton
        {
            get => mSingleton;
            set
            {
                this.mSingleton = value;
                this.Dirty = true;
            }
        }

        public int Quantity
        {
            get => mQuantity;
            set
            {
                this.mQuantity = value;
                this.Dirty = true;
            }
        }

        public double X
        {
            get => mX;
            set
            {
                this.mX = value;
                this.Dirty = true;
            }
        }

        public double Y
        {
            get => mY;
            set
            {
                this.mY = value;
                this.Dirty = true;
            }
        }

        public double Z
        {
            get => mZ;
            set
            {
                this.mZ = value;
                this.Dirty = true;
            }
        }

        public string CustomInfo
        {
            get => mCustomInfo;
            set
            {
                this.mCustomInfo = value;
                this.Dirty = true;
            }
        }

        public Entity(string entityName, int entityId, ItemType type, int entityOwnerID, int entityLocationID, int entityFlag, bool entityContraband, bool entitySingleton, int entityQuantity, double entityX, double entityY, double entityZ, string entityCustomInfo, AttributeList attributes, ItemFactory itemFactory)
        {
            this.mName = entityName;
            this.mID = entityId;
            this.mType = type;
            this.mOwnerID = entityOwnerID;
            this.mLocationID = entityLocationID;
            this.mFlag = entityFlag;
            this.mContraband = entityContraband;
            this.mSingleton = entitySingleton;
            this.mQuantity = entityQuantity;
            this.mX = entityX;
            this.mY = entityY;
            this.mZ = entityZ;
            this.mCustomInfo = entityCustomInfo;
            this.mAttributes = attributes;

            this.mItemFactory = itemFactory;
        }

        protected override void SaveToDB()
        {
            // entities cannot be "new" as these have to be created in the database before instantiation
            // of this class, so the "New" flag can be ignored
            this.mItemFactory.ItemDB.PersistEntity(this);
        }

        public override void Persist()
        {
            // persist this object if needed
            base.Persist();
            
            // persist the attribute list too
            this.Attributes.Persist(this);
        }
    }
}
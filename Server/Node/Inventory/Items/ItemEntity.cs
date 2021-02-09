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

using System.Collections.Generic;
using Common.Database;
using Node.Inventory.Items.Attributes;
using PythonTypes.Types.Database;
using PythonTypes.Types.Primitives;

namespace Node.Inventory.Items
{
    public abstract class ItemEntity : DatabaseEntity
    {
        public ItemFactory mItemFactory = null;

        public static DBRowDescriptor sEntityItemDescriptor = new DBRowDescriptor()
        {
            Columns =
            {
                new DBRowDescriptor.Column("itemID", FieldType.I4),
                new DBRowDescriptor.Column("typeID", FieldType.I2),
                new DBRowDescriptor.Column("ownerID", FieldType.I4),
                new DBRowDescriptor.Column("locationID", FieldType.I4),
                new DBRowDescriptor.Column("flag", FieldType.UI1),
                new DBRowDescriptor.Column("contraband", FieldType.Bool),
                new DBRowDescriptor.Column("singleton", FieldType.Bool),
                new DBRowDescriptor.Column("quantity", FieldType.I4),
                new DBRowDescriptor.Column("groupID", FieldType.I2),
                new DBRowDescriptor.Column("categoryID", FieldType.UI1),
                new DBRowDescriptor.Column("customInfo", FieldType.Str)
            }
        };

        private int mID;
        private string mName;
        private ItemType mType;
        private int mOwnerID;
        private int mLocationID;
        private ItemFlags mFlag;
        private bool mContraband;
        private bool mSingleton;
        private int mQuantity; // TODO: DEPRECATE THIS AND USE QUANTITY ATTRIBUTE
        private double mX;
        private double mY;
        private double mZ;
        private string mCustomInfo;
        private AttributeList mAttributes;

        public int ID => mID;
        public AttributeList Attributes => mAttributes;

        public ItemType Type
        {
            get => this.mType;
            set
            {
                this.mType = value;
                this.Dirty = true;
            }
        }

        public string Name
        {
            get => mName ?? Type.Name;
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

        public ItemFlags Flag
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

        public bool HasName => this.mName != null;
        
        public ItemEntity(string entityName, int entityId, ItemType type, int ownerID,
            int locationID, ItemFlags entityFlag, bool entityContraband, bool entitySingleton,
            int entityQuantity, double entityX, double entityY, double entityZ, string entityCustomInfo,
            AttributeList attributes, ItemFactory itemFactory)
        {
            this.mName = entityName;
            this.mID = entityId;
            this.mType = type;
            this.mOwnerID = ownerID;
            this.mLocationID = locationID;
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

        public ItemEntity(string entityName, int entityId, ItemType type, ItemEntity entityOwner,
            ItemEntity entityLocation, ItemFlags entityFlag, bool entityContraband, bool entitySingleton,
            int entityQuantity, double entityX, double entityY, double entityZ, string entityCustomInfo,
            AttributeList attributes, ItemFactory itemFactory) : this(
            entityName, entityId, type, entityOwner.ID, entityLocation.ID, entityFlag, entityContraband,
            entitySingleton, entityQuantity, entityX, entityY, entityZ, entityCustomInfo, attributes, itemFactory)
        {
        }

        public ItemEntity(ItemEntity from) : this(from.Name, from.ID, from.Type, from.OwnerID, from.LocationID, from.Flag,
            from.Contraband, from.Singleton, from.Quantity, from.X, from.Y, from.Z, from.CustomInfo, from.Attributes,
            from.mItemFactory)
        {
        }

        protected override void SaveToDB()
        {
            this.mItemFactory.ItemDB.PersistEntity(this);
        }

        public override void Persist()
        {
            // persist is overriden so the attributes are persisted regardless of the Dirty flag
            base.Persist();
            
            // persist the attribute list too
            this.Attributes.Persist(this);
        }

        public override void Destroy()
        {
            base.Destroy();

            this.mItemFactory.ItemDB.DestroyItem(this);
        }

        public PyPackedRow GetEntityRow()
        {
            Dictionary<string, PyDataType> values = new Dictionary<string, PyDataType> ()
            {
                {"itemID", this.ID},
                {"typeID", this.Type.ID},
                {"locationID", this.LocationID},
                {"ownerID", this.OwnerID},
                {"flag", (int) this.Flag},
                {"contraband", this.Contraband},
                {"singleton", this.Singleton},
                {"quantity", this.Quantity},
                {"groupID", this.Type.Group.ID},
                {"categoryID", this.Type.Group.Category.ID},
                {"customInfo", this.CustomInfo}
            };
            
            return new PyPackedRow(sEntityItemDescriptor, values);
        }
        
        public PyDictionary GetEffects()
        {
            // for now return no data
            return new PyDictionary();
        }
    }
}
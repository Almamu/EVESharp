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
using System.Collections.Generic;
using Common.Database;
using Node.Exceptions.Internal;
using Node.Exceptions.jumpCloneSvc;
using Node.Exceptions.ship;
using Node.Inventory.Items.Attributes;
using Node.Inventory.Items.Types;
using PythonTypes.Types.Collections;
using PythonTypes.Types.Database;
using PythonTypes.Types.Primitives;

namespace Node.Inventory.Items
{
    public abstract class ItemEntity : DatabaseEntity
    {
        public ItemFactory ItemFactory { get; }

        public static readonly DBRowDescriptor sEntityItemDescriptor = new DBRowDescriptor()
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
        private double? mX;
        private double? mY;
        private double? mZ;
        private string mCustomInfo;
        private AttributeList mAttributes;
        private bool mHadName;
        private bool mHadPosition;

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

        public double? X
        {
            get => mX;
            set
            {
                this.mX = value;
                this.Dirty = true;
            }
        }

        public double? Y
        {
            get => mY;
            set
            {
                this.mY = value;
                this.Dirty = true;
            }
        }

        public double? Z
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

        public bool HasName => this.mName is not null;
        public virtual bool HasPosition => this.X is not null && this.Y is not null && this.Z is not null;
        public bool HadName => this.mHadName;
        public bool HadPosition => this.mHadPosition;
        
        public ItemEntity(string entityName, int entityId, ItemType type, int ownerID,
            int locationID, ItemFlags entityFlag, bool entityContraband, bool entitySingleton,
            int entityQuantity, double? x, double? y, double? z, string entityCustomInfo, AttributeList attributes, ItemFactory itemFactory)
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
            this.mCustomInfo = entityCustomInfo;
            this.mAttributes = attributes;
            this.mX = x;
            this.mY = y;
            this.mZ = z;

            this.ItemFactory = itemFactory;
            this.mHadName = entityName is not null;
            this.mHadPosition = x is not null && y is not null && z is not null;
        }

        public ItemEntity(string entityName, int entityId, ItemType type, ItemEntity entityOwner,
            ItemEntity entityLocation, ItemFlags entityFlag, bool entityContraband, bool entitySingleton,
            int entityQuantity, double? x, double? y, double? z, string entityCustomInfo, AttributeList attributes, ItemFactory itemFactory) : this(
            entityName, entityId, type, entityOwner.ID, entityLocation.ID, entityFlag, entityContraband,
            entitySingleton, entityQuantity, x, y, z, entityCustomInfo, attributes, itemFactory)
        {
        }

        public ItemEntity(ItemEntity from) : this(from.HasName ? from.Name : null, from.ID, from.Type, from.OwnerID, from.LocationID, from.Flag,
            from.Contraband, from.Singleton, from.Quantity, from.X, from.Y, from.Z, from.CustomInfo, from.Attributes,
            from.ItemFactory)
        {
        }

        protected override void SaveToDB()
        {
            this.ItemFactory.ItemDB.PersistEntity(this);
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

            this.ItemFactory.ItemDB.DestroyItem(this);
        }

        public override void Dispose()
        {
            // persist the item to the database
            this.Persist();
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

        protected void CheckSkillRequirement(AttributeEnum skillTypeIDRequirement, AttributeEnum skillLevelRequirement, Dictionary<int, Skill> skills)
        {
            if (this.Attributes.AttributeExists(skillLevelRequirement) == false ||
                this.Attributes.AttributeExists(skillTypeIDRequirement) == false)
                return;

            int skillTypeID = (int) this.Attributes[skillTypeIDRequirement];
            int skillLevel = (int) this.Attributes[skillLevelRequirement];

            if (skills.ContainsKey(skillTypeID) == false)
                throw new SkillMissingException(this.ItemFactory.TypeManager[skillTypeID].Name);

            if (skills[skillTypeID].Level < skillLevel)
                throw new SkillMissingException(this.ItemFactory.TypeManager[skillTypeID].Name);
        }

        public virtual void CheckPrerequisites(Character character)
        {
            Dictionary<int, Skill> skills = character.InjectedSkillsByTypeID;
            List<string> missingSkills = new List<string>();
            AttributeEnum[] attributes = new AttributeEnum[]
            {
                AttributeEnum.requiredSkill1,
                AttributeEnum.requiredSkill2,
                AttributeEnum.requiredSkill3,
                AttributeEnum.requiredSkill4,
                AttributeEnum.requiredSkill5,
                AttributeEnum.requiredSkill6,
            };
            AttributeEnum[] levelAttributes = new AttributeEnum[]
            {
                AttributeEnum.requiredSkill1Level,
                AttributeEnum.requiredSkill2Level,
                AttributeEnum.requiredSkill3Level,
                AttributeEnum.requiredSkill4Level,
                AttributeEnum.requiredSkill5Level,
                AttributeEnum.requiredSkill6Level,
            };

            for (int i = 0; i < attributes.Length; i++)
            {
                try
                {
                    this.CheckSkillRequirement(attributes[i], levelAttributes[i], skills);
                }
                catch (SkillMissingException e)
                {
                    missingSkills.Add(e.SkillName);
                }
            }

            if (missingSkills.Count > 0)
                throw new ShipHasSkillPrerequisites(this.Type.Name, String.Join(", ", missingSkills));
        }

        public void EnsureOwnership(Character character)
        {
            if (this.OwnerID != character.ID)
                throw new MktNotOwner();
        }
    }
}
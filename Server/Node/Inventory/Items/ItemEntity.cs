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
using Node.Exceptions.corpRegistry;
using Node.Exceptions.Internal;
using Node.Exceptions.jumpCloneSvc;
using Node.Exceptions.ship;
using Node.Inventory.Items.Attributes;
using Node.Inventory.Items.Types;
using Node.StaticData.Corporation;
using Node.StaticData.Inventory;
using PythonTypes.Types.Collections;
using PythonTypes.Types.Database;
using PythonTypes.Types.Primitives;
using Type = Node.StaticData.Inventory.Type;

namespace Node.Inventory.Items
{
    public abstract class ItemEntity : DatabaseEntity
    {
        public ItemFactory ItemFactory { get; }

        public static readonly DBRowDescriptor EntityItemDescriptor = new DBRowDescriptor()
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
        private Type mType;
        private int mOwnerID;
        private int mLocationID;
        private Flags mFlag;
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

        public Type Type
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

        public virtual int OwnerID
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

        public Flags Flag
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
        
        public ItemEntity(string entityName, int entityId, Type type, int ownerID,
            int locationID, Flags entityFlag, bool entityContraband, bool entitySingleton,
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

        public ItemEntity(string entityName, int entityId, Type type, ItemEntity entityOwner,
            ItemEntity entityLocation, Flags entityFlag, bool entityContraband, bool entitySingleton,
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
            
            return new PyPackedRow(EntityItemDescriptor, values);
        }
        
        public virtual PyDictionary GetEffects()
        {
            // for now return no data
            return new PyDictionary();
        }

        protected void CheckSkillRequirement(StaticData.Inventory.Attributes skillTypeIDRequirement, StaticData.Inventory.Attributes skillLevelRequirement, Dictionary<int, Skill> skills)
        {
            if (this.Attributes.AttributeExists(skillLevelRequirement) == false ||
                this.Attributes.AttributeExists(skillTypeIDRequirement) == false)
                return;

            int skillTypeID = (int) this.Attributes[skillTypeIDRequirement];
            int skillLevel = (int) this.Attributes[skillLevelRequirement];

            if (skills.ContainsKey(skillTypeID) == false)
                throw new SkillMissingException(this.ItemFactory.TypeManager[skillTypeID]);

            if (skills[skillTypeID].Level < skillLevel)
                throw new SkillMissingException(this.ItemFactory.TypeManager[skillTypeID]);
        }

        public virtual void CheckPrerequisites(Character character)
        {
            Dictionary<int, Skill> skills = character.InjectedSkillsByTypeID;
            PyList<PyInteger> missingSkills = new PyList<PyInteger>();
            StaticData.Inventory.Attributes[] attributes = new StaticData.Inventory.Attributes[]
            {
                StaticData.Inventory.Attributes.requiredSkill1,
                StaticData.Inventory.Attributes.requiredSkill2,
                StaticData.Inventory.Attributes.requiredSkill3,
                StaticData.Inventory.Attributes.requiredSkill4,
                StaticData.Inventory.Attributes.requiredSkill5,
                StaticData.Inventory.Attributes.requiredSkill6,
            };
            StaticData.Inventory.Attributes[] levelAttributes = new StaticData.Inventory.Attributes[]
            {
                StaticData.Inventory.Attributes.requiredSkill1Level,
                StaticData.Inventory.Attributes.requiredSkill2Level,
                StaticData.Inventory.Attributes.requiredSkill3Level,
                StaticData.Inventory.Attributes.requiredSkill4Level,
                StaticData.Inventory.Attributes.requiredSkill5Level,
                StaticData.Inventory.Attributes.requiredSkill6Level,
            };

            for (int i = 0; i < attributes.Length; i++)
            {
                try
                {
                    this.CheckSkillRequirement(attributes[i], levelAttributes[i], skills);
                }
                catch (SkillMissingException e)
                {
                    missingSkills.Add(e.Skill.ID);
                }
            }

            if (missingSkills.Count > 0)
                throw new ShipHasSkillPrerequisites(this.Type, missingSkills);
        }

        public void EnsureOwnership(int characterID, int corporationID, long corporationRole, bool take = false)
        {
            if (this.OwnerID == characterID)
                return;

            if (this.OwnerID != corporationID)
                throw new MktNotOwner();

            if (take == true)
            {
                if (this.Flag == Flags.CorpMarket && CorporationRole.Trader.Is(corporationRole) == true)
                    return;
                if (this.Flag == Flags.Hangar && CorporationRole.HangarCanTake1.Is(corporationRole) == true)
                    return;
                if (this.Flag == Flags.CorpSAG2 && CorporationRole.HangarCanTake2.Is(corporationRole) == true)
                    return;
                if (this.Flag == Flags.CorpSAG3 && CorporationRole.HangarCanTake3.Is(corporationRole) == true)
                    return;
                if (this.Flag == Flags.CorpSAG4 && CorporationRole.HangarCanTake4.Is(corporationRole) == true)
                    return;
                if (this.Flag == Flags.CorpSAG5 && CorporationRole.HangarCanTake5.Is(corporationRole) == true)
                    return;
                if (this.Flag == Flags.CorpSAG6 && CorporationRole.HangarCanTake6.Is(corporationRole) == true)
                    return;
                if (this.Flag == Flags.CorpSAG7 && CorporationRole.HangarCanTake7.Is(corporationRole) == true)
                    return;
            }
            else
            {
                if (this.Flag == Flags.CorpMarket && CorporationRole.Trader.Is(corporationRole) == true)
                    return;
                if (this.Flag == Flags.Hangar && CorporationRole.HangarCanQuery1.Is(corporationRole) == true)
                    return;
                if (this.Flag == Flags.CorpSAG2 && CorporationRole.HangarCanQuery2.Is(corporationRole) == true)
                    return;
                if (this.Flag == Flags.CorpSAG3 && CorporationRole.HangarCanQuery3.Is(corporationRole) == true)
                    return;
                if (this.Flag == Flags.CorpSAG4 && CorporationRole.HangarCanQuery4.Is(corporationRole) == true)
                    return;
                if (this.Flag == Flags.CorpSAG5 && CorporationRole.HangarCanQuery5.Is(corporationRole) == true)
                    return;
                if (this.Flag == Flags.CorpSAG6 && CorporationRole.HangarCanQuery6.Is(corporationRole) == true)
                    return;
                if (this.Flag == Flags.CorpSAG7 && CorporationRole.HangarCanQuery7.Is(corporationRole) == true)
                    return;
            }

            throw new MktNotOwner();
        }

        public bool IsInModuleSlot()
        {
            return this.Flag == Flags.HiSlot0 || this.Flag == Flags.HiSlot1 || this.Flag == Flags.HiSlot2 ||
                   this.Flag == Flags.HiSlot3 || this.Flag == Flags.HiSlot4 || this.Flag == Flags.HiSlot5 ||
                   this.Flag == Flags.HiSlot6 || this.Flag == Flags.HiSlot7 || this.Flag == Flags.MedSlot0 ||
                   this.Flag == Flags.MedSlot1 || this.Flag == Flags.MedSlot2 || this.Flag == Flags.MedSlot3 ||
                   this.Flag == Flags.MedSlot4 || this.Flag == Flags.MedSlot5 || this.Flag == Flags.MedSlot6 ||
                   this.Flag == Flags.MedSlot7 || this.Flag == Flags.LoSlot0 || this.Flag == Flags.LoSlot1 ||
                   this.Flag == Flags.LoSlot2 || this.Flag == Flags.LoSlot3 || this.Flag == Flags.LoSlot4 ||
                   this.Flag == Flags.LoSlot5 || this.Flag == Flags.LoSlot6 || this.Flag == Flags.LoSlot7;
        }

        public bool IsInRigSlot()
        {
            return this.Flag == Flags.RigSlot0 || this.Flag == Flags.RigSlot1 || this.Flag == Flags.RigSlot2 ||
                   this.Flag == Flags.RigSlot3 || this.Flag == Flags.RigSlot4 || this.Flag == Flags.RigSlot5 ||
                   this.Flag == Flags.RigSlot6 || this.Flag == Flags.RigSlot7;
        }
    }
}
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
using EVESharp.Common.Database;
using EVESharp.Node.Exceptions.Internal;
using EVESharp.Node.Exceptions.jumpCloneSvc;
using EVESharp.Node.Exceptions.ship;
using EVESharp.Node.Inventory.Items.Attributes;
using EVESharp.Node.Inventory.Items.Types;
using EVESharp.Node.StaticData.Corporation;
using EVESharp.Node.StaticData.Inventory;
using EVESharp.Node.Exceptions.corpRegistry;
using EVESharp.PythonTypes.Types.Collections;
using EVESharp.PythonTypes.Types.Database;
using EVESharp.PythonTypes.Types.Primitives;
using Type = EVESharp.Node.StaticData.Inventory.Type;
using Information = EVESharp.Node.Inventory.Items.Types.Information;
namespace EVESharp.Node.Inventory.Items
{
    public delegate void ItemEventHandler(ItemEntity sender);
    
    public abstract class ItemEntity : IDisposable
    {
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
        
        /// <summary>
        /// Indicates if the object is new in the database or not
        /// </summary>
        public bool New { get; set; }
        
        /// <summary>
        /// Holds the actual item's information
        /// </summary>
        public Information.Item Information { get; }
        
        private bool mHadName;
        private bool mHadPosition;

        public int ID => this.Information.ID;
        public AttributeList Attributes => this.Information.Attributes;
        public Type Type => this.Information.Type;
        public string Name
        {
            get => this.Information.Name ?? Type.Name;
            set 
            {
                this.Information.Name = value;
                this.Information.Dirty = true;
            }
        }

        public virtual int OwnerID
        {
            get => this.Information.OwnerID;
            set
            {
                this.Information.OwnerID = value;
                this.Information.Dirty = true;
            }
        }

        public int LocationID
        {
            get => this.Information.LocationID;
            set
            {
                this.Information.LocationID = value;
                this.Information.Dirty = true;
            }
        }

        public Flags Flag
        {
            get => this.Information.Flag;
            set
            {
                this.Information.Flag = value;
                this.Information.Dirty = true;
            }
        }

        public bool Contraband
        {
            get => this.Information.Contraband;
            set
            {
                this.Information.Contraband = value;
                this.Information.Dirty = true;
            }
        }

        public bool Singleton
        {
            get => this.Information.Singleton;
            set
            {
                this.Information.Singleton = value;
                this.Information.Dirty = true;
            }
        }

        public int Quantity
        {
            get => this.Information.Quantity;
            set
            {
                this.Information.Quantity = value;
                this.Information.Dirty = true;
            }
        }

        public double? X
        {
            get => this.Information.X;
            set
            {
                this.Information.X = value;
                this.Information.Dirty = true;
            }
        }

        public double? Y
        {
            get => this.Information.Y;
            set
            {
                this.Information.Y = value;
                this.Information.Dirty = true;
            }
        }

        public double? Z
        {
            get => this.Information.Z;
            set
            {
                this.Information.Z = value;
                this.Information.Dirty = true;
            }
        }

        public string CustomInfo
        {
            get => this.Information.CustomInfo;
            set
            {
                this.Information.CustomInfo = value;
                this.Information.Dirty = true;
            }
        }

        public bool HasName => this.Information.Name is not null;
        public virtual bool HasPosition => this.X is not null && this.Y is not null && this.Z is not null;
        public bool HadName => this.mHadName;
        public bool HadPosition => this.mHadPosition;
        
        /// <summary>
        /// Event called by the item when it's destroyed
        /// </summary>
        public ItemEventHandler OnItemDestroyed;
        /// <summary>
        /// Event called by the item when it's disposed of
        /// </summary>
        public ItemEventHandler OnItemDisposed;
        /// <summary>
        /// Event called by the item when it's persisted to the database
        /// </summary>
        public ItemEventHandler OnItemPersisted;
        
        public ItemEntity(Information.Item info)
        {
            this.Information = info;
            
            this.mHadName = this.Information.Name is not null;
            this.mHadPosition = this.Information.X is not null && this.Information.Y is not null && this.Information.Z is not null;
        }

        protected ItemEntity(ItemEntity from) : this(from.Information)
        {
            // keep the status of the original name and position indications
            this.mHadName = from.mHadName;
            this.mHadPosition = from.mHadPosition;
        }

        public virtual void Persist()
        {
            this.OnItemPersisted?.Invoke(this);
        }

        public virtual void Destroy()
        {
            this.OnItemDestroyed?.Invoke(this);
        }

        public virtual void Dispose()
        {
            // ensure things are persisted
            this.Persist();
            
            // fire the dispose event
            this.OnItemDisposed?.Invoke(this);
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
                throw new SkillMissingException(skillTypeID);

            if (skills[skillTypeID].Level < skillLevel)
                throw new SkillMissingException(skillTypeID);
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
                    missingSkills.Add(e.SkillTypeID);
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
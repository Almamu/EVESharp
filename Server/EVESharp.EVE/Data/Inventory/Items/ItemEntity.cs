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
using EVESharp.Database.Corporations;
using EVESharp.Database.Inventory;
using EVESharp.Database.Inventory.Attributes;
using EVESharp.EVE.Data.Inventory.Items.Types;
using EVESharp.EVE.Exceptions.Internal;
using EVESharp.EVE.Exceptions.jumpCloneSvc;
using EVESharp.EVE.Exceptions.ship;
using EVESharp.Types;
using EVESharp.Types.Collections;
using Item = EVESharp.Database.Inventory.Types.Information.Item;
using Type = EVESharp.Database.Inventory.Types.Type;

namespace EVESharp.EVE.Data.Inventory.Items;

public abstract class ItemEntity : IDisposable
{
    public static readonly DBRowDescriptor EntityItemDescriptor = new DBRowDescriptor
    {
        Columns =
        {
            new DBRowDescriptor.Column ("itemID",     FieldType.I4),
            new DBRowDescriptor.Column ("typeID",     FieldType.I2),
            new DBRowDescriptor.Column ("ownerID",    FieldType.I4),
            new DBRowDescriptor.Column ("locationID", FieldType.I4),
            new DBRowDescriptor.Column ("flag",       FieldType.UI1),
            new DBRowDescriptor.Column ("contraband", FieldType.Bool),
            new DBRowDescriptor.Column ("singleton",  FieldType.Bool),
            new DBRowDescriptor.Column ("quantity",   FieldType.I4),
            new DBRowDescriptor.Column ("groupID",    FieldType.I2),
            new DBRowDescriptor.Column ("categoryID", FieldType.UI1),
            new DBRowDescriptor.Column ("customInfo", FieldType.Str)
        }
    };

    /// <summary>
    /// Event called by the item when it's destroyed
    /// </summary>
    public event Action<ItemEntity> Destroyed;
    /// <summary>
    /// Event called by the item when it's disposed of
    /// </summary>
    public event Action<ItemEntity> Disposed;
    /// <summary>
    /// Event called by the item when it's persisted to the database
    /// </summary>
    public event Action<ItemEntity> Persisted;

    /// <summary>
    /// Indicates if the object is new in the database or not
    /// </summary>
    public bool New { get; set; }

    /// <summary>
    /// Holds the actual item's information
    /// </summary>
    public Item Information { get; }

    public int           ID         => this.Information.ID;
    public AttributeList Attributes => this.Information.Attributes;
    public Type          Type       => this.Information.Type;
    public string Name
    {
        get => this.Information.Name ?? this.Type.Name;
        set
        {
            this.Information.Name  = value;
            this.Information.Dirty = true;
        }
    }

    public virtual int OwnerID
    {
        get => this.Information.OwnerID;
        set
        {
            this.Information.OwnerID = value;
            this.Information.Dirty   = true;
        }
    }

    public int LocationID
    {
        get => this.Information.LocationID;
        set
        {
            this.Information.LocationID = value;
            this.Information.Dirty      = true;
        }
    }

    public Flags Flag
    {
        get => this.Information.Flag;
        set
        {
            this.Information.Flag  = value;
            this.Information.Dirty = true;
        }
    }

    public bool Contraband
    {
        get => this.Information.Contraband;
        set
        {
            this.Information.Contraband = value;
            this.Information.Dirty      = true;
        }
    }

    public bool Singleton
    {
        get => this.Information.Singleton;
        set
        {
            this.Information.Singleton = value;
            this.Information.Dirty     = true;
        }
    }

    public int Quantity
    {
        get => this.Information.Quantity;
        set
        {
            this.Information.Quantity = value;
            this.Information.Dirty    = true;
        }
    }

    public double? X
    {
        get => this.Information.X;
        set
        {
            this.Information.X     = value;
            this.Information.Dirty = true;
        }
    }

    public double? Y
    {
        get => this.Information.Y;
        set
        {
            this.Information.Y     = value;
            this.Information.Dirty = true;
        }
    }

    public double? Z
    {
        get => this.Information.Z;
        set
        {
            this.Information.Z     = value;
            this.Information.Dirty = true;
        }
    }

    public string CustomInfo
    {
        get => this.Information.CustomInfo;
        set
        {
            this.Information.CustomInfo = value;
            this.Information.Dirty      = true;
        }
    }

    public         bool HasName     => this.Information.Name is not null;
    public virtual bool HasPosition => this.X is not null && this.Y is not null && this.Z is not null;
    public         bool HadName     { get; }
    public         bool HadPosition { get; }

    public ItemEntity (Item info)
    {
        this.Information = info;

        this.HadName     = this.Information.Name is not null;
        this.HadPosition = this.Information.X is not null && this.Information.Y is not null && this.Information.Z is not null;
    }

    protected ItemEntity (ItemEntity from) : this (from.Information)
    {
        // keep the status of the original name and position indications
        this.HadName     = from.HadName;
        this.HadPosition = from.HadPosition;
    }

    public virtual void Dispose ()
    {
        // ensure things are persisted
        this.Persist ();

        // fire the dispose event
        this.Disposed?.Invoke (this);
    }

    public virtual void Persist ()
    {
        this.Persisted?.Invoke (this);
    }

    public virtual void Destroy ()
    {
        this.Destroyed?.Invoke (this);
    }

    public PyPackedRow GetEntityRow ()
    {
        Dictionary <string, PyDataType> values = new Dictionary <string, PyDataType>
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

        return new PyPackedRow (EntityItemDescriptor, values);
    }

    public virtual PyDictionary GetEffects ()
    {
        // for now return no data
        return new PyDictionary ();
    }

    protected void CheckSkillRequirement (AttributeTypes skillTypeIDRequirement, AttributeTypes skillLevelRequirement, Dictionary <int, Skill> skills)
    {
        if (this.Attributes.AttributeExists (skillLevelRequirement) == false ||
            this.Attributes.AttributeExists (skillTypeIDRequirement) == false)
            return;

        int skillTypeID = (int) this.Attributes [skillTypeIDRequirement];
        int skillLevel  = (int) this.Attributes [skillLevelRequirement];

        if (skills.ContainsKey (skillTypeID) == false)
            throw new SkillMissingException (skillTypeID);

        if (skills [skillTypeID].Level < skillLevel)
            throw new SkillMissingException (skillTypeID);
    }

    public virtual void CheckPrerequisites (Character character)
    {
        Dictionary <int, Skill> skills        = character.InjectedSkillsByTypeID;
        PyList <PyInteger>      missingSkills = new PyList <PyInteger> ();
        AttributeTypes [] attributes =
        {
            AttributeTypes.requiredSkill1,
            AttributeTypes.requiredSkill2,
            AttributeTypes.requiredSkill3,
            AttributeTypes.requiredSkill4,
            AttributeTypes.requiredSkill5,
            AttributeTypes.requiredSkill6
        };
        AttributeTypes [] levelAttributes =
        {
            AttributeTypes.requiredSkill1Level,
            AttributeTypes.requiredSkill2Level,
            AttributeTypes.requiredSkill3Level,
            AttributeTypes.requiredSkill4Level,
            AttributeTypes.requiredSkill5Level,
            AttributeTypes.requiredSkill6Level
        };

        for (int i = 0; i < attributes.Length; i++)
            try
            {
                this.CheckSkillRequirement (attributes [i], levelAttributes [i], skills);
            }
            catch (SkillMissingException e)
            {
                missingSkills.Add (e.SkillTypeID);
            }

        if (missingSkills.Count > 0)
            throw new ShipHasSkillPrerequisites (this.Type, missingSkills);
    }

    public void EnsureOwnership (int characterID, int corporationID, long corporationRole, bool take = false)
    {
        if (this.OwnerID == characterID)
            return;

        if (this.OwnerID != corporationID)
            throw new MktNotOwner ();

        if (take)
        {
            if (this.Flag == Flags.CorpMarket && CorporationRole.Trader.Is (corporationRole))
                return;
            if (this.Flag == Flags.Hangar && CorporationRole.HangarCanTake1.Is (corporationRole))
                return;
            if (this.Flag == Flags.CorpSAG2 && CorporationRole.HangarCanTake2.Is (corporationRole))
                return;
            if (this.Flag == Flags.CorpSAG3 && CorporationRole.HangarCanTake3.Is (corporationRole))
                return;
            if (this.Flag == Flags.CorpSAG4 && CorporationRole.HangarCanTake4.Is (corporationRole))
                return;
            if (this.Flag == Flags.CorpSAG5 && CorporationRole.HangarCanTake5.Is (corporationRole))
                return;
            if (this.Flag == Flags.CorpSAG6 && CorporationRole.HangarCanTake6.Is (corporationRole))
                return;
            if (this.Flag == Flags.CorpSAG7 && CorporationRole.HangarCanTake7.Is (corporationRole))
                return;
        }
        else
        {
            if (this.Flag == Flags.CorpMarket && CorporationRole.Trader.Is (corporationRole))
                return;
            if (this.Flag == Flags.Hangar && CorporationRole.HangarCanQuery1.Is (corporationRole))
                return;
            if (this.Flag == Flags.CorpSAG2 && CorporationRole.HangarCanQuery2.Is (corporationRole))
                return;
            if (this.Flag == Flags.CorpSAG3 && CorporationRole.HangarCanQuery3.Is (corporationRole))
                return;
            if (this.Flag == Flags.CorpSAG4 && CorporationRole.HangarCanQuery4.Is (corporationRole))
                return;
            if (this.Flag == Flags.CorpSAG5 && CorporationRole.HangarCanQuery5.Is (corporationRole))
                return;
            if (this.Flag == Flags.CorpSAG6 && CorporationRole.HangarCanQuery6.Is (corporationRole))
                return;
            if (this.Flag == Flags.CorpSAG7 && CorporationRole.HangarCanQuery7.Is (corporationRole))
                return;
        }

        throw new MktNotOwner ();
    }

    public bool IsInModuleSlot ()
    {
        return this.Flag.IsHighModule () || this.Flag.IsMediumModule () || this.Flag.IsLowModule ();
    }

    public bool IsInRigSlot ()
    {
        return this.Flag.IsRigModule ();
    }
}
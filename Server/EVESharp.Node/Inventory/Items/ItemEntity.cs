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
using EVESharp.EVE.Data.Corporation;
using EVESharp.EVE.Data.Inventory;
using EVESharp.EVE.Exceptions.Internal;
using EVESharp.EVE.Exceptions.jumpCloneSvc;
using EVESharp.EVE.Exceptions.ship;
using EVESharp.EVE.Inventory.Attributes;
using EVESharp.Node.Inventory.Items.Types;
using EVESharp.PythonTypes.Types.Collections;
using EVESharp.PythonTypes.Types.Database;
using EVESharp.PythonTypes.Types.Primitives;
using Item = EVESharp.Node.Inventory.Items.Types.Information.Item;
using Type = EVESharp.EVE.Data.Inventory.Type;

namespace EVESharp.Node.Inventory.Items;

public delegate void ItemEventHandler (ItemEntity sender);

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
    public ItemEventHandler OnItemDestroyed;
    /// <summary>
    /// Event called by the item when it's disposed of
    /// </summary>
    public ItemEventHandler OnItemDisposed;
    /// <summary>
    /// Event called by the item when it's persisted to the database
    /// </summary>
    public ItemEventHandler OnItemPersisted;

    /// <summary>
    /// Indicates if the object is new in the database or not
    /// </summary>
    public bool New { get; set; }

    /// <summary>
    /// Holds the actual item's information
    /// </summary>
    public Item Information { get; }

    public int           ID         => Information.ID;
    public AttributeList Attributes => Information.Attributes;
    public Type          Type       => Information.Type;
    public string Name
    {
        get => Information.Name ?? Type.Name;
        set
        {
            Information.Name  = value;
            Information.Dirty = true;
        }
    }

    public virtual int OwnerID
    {
        get => Information.OwnerID;
        set
        {
            Information.OwnerID = value;
            Information.Dirty   = true;
        }
    }

    public int LocationID
    {
        get => Information.LocationID;
        set
        {
            Information.LocationID = value;
            Information.Dirty      = true;
        }
    }

    public Flags Flag
    {
        get => Information.Flag;
        set
        {
            Information.Flag  = value;
            Information.Dirty = true;
        }
    }

    public bool Contraband
    {
        get => Information.Contraband;
        set
        {
            Information.Contraband = value;
            Information.Dirty      = true;
        }
    }

    public bool Singleton
    {
        get => Information.Singleton;
        set
        {
            Information.Singleton = value;
            Information.Dirty     = true;
        }
    }

    public int Quantity
    {
        get => Information.Quantity;
        set
        {
            Information.Quantity = value;
            Information.Dirty    = true;
        }
    }

    public double? X
    {
        get => Information.X;
        set
        {
            Information.X     = value;
            Information.Dirty = true;
        }
    }

    public double? Y
    {
        get => Information.Y;
        set
        {
            Information.Y     = value;
            Information.Dirty = true;
        }
    }

    public double? Z
    {
        get => Information.Z;
        set
        {
            Information.Z     = value;
            Information.Dirty = true;
        }
    }

    public string CustomInfo
    {
        get => Information.CustomInfo;
        set
        {
            Information.CustomInfo = value;
            Information.Dirty      = true;
        }
    }

    public         bool HasName     => Information.Name is not null;
    public virtual bool HasPosition => X is not null && Y is not null && Z is not null;
    public         bool HadName     { get; }
    public         bool HadPosition { get; }

    public ItemEntity (Item info)
    {
        Information = info;

        HadName     = Information.Name is not null;
        HadPosition = Information.X is not null && Information.Y is not null && Information.Z is not null;
    }

    protected ItemEntity (ItemEntity from) : this (from.Information)
    {
        // keep the status of the original name and position indications
        HadName     = from.HadName;
        HadPosition = from.HadPosition;
    }

    public virtual void Dispose ()
    {
        // ensure things are persisted
        this.Persist ();

        // fire the dispose event
        this.OnItemDisposed?.Invoke (this);
    }

    public virtual void Persist ()
    {
        this.OnItemPersisted?.Invoke (this);
    }

    public virtual void Destroy ()
    {
        this.OnItemDestroyed?.Invoke (this);
    }

    public PyPackedRow GetEntityRow ()
    {
        Dictionary <string, PyDataType> values = new Dictionary <string, PyDataType>
        {
            {"itemID", ID},
            {"typeID", Type.ID},
            {"locationID", LocationID},
            {"ownerID", OwnerID},
            {"flag", (int) Flag},
            {"contraband", Contraband},
            {"singleton", Singleton},
            {"quantity", Quantity},
            {"groupID", Type.Group.ID},
            {"categoryID", Type.Group.Category.ID},
            {"customInfo", CustomInfo}
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
        if (Attributes.AttributeExists (skillLevelRequirement) == false ||
            Attributes.AttributeExists (skillTypeIDRequirement) == false)
            return;

        int skillTypeID = (int) Attributes [skillTypeIDRequirement];
        int skillLevel  = (int) Attributes [skillLevelRequirement];

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
            throw new ShipHasSkillPrerequisites (Type, missingSkills);
    }

    public void EnsureOwnership (int characterID, int corporationID, long corporationRole, bool take = false)
    {
        if (OwnerID == characterID)
            return;

        if (OwnerID != corporationID)
            throw new MktNotOwner ();

        if (take)
        {
            if (Flag == Flags.CorpMarket && CorporationRole.Trader.Is (corporationRole))
                return;
            if (Flag == Flags.Hangar && CorporationRole.HangarCanTake1.Is (corporationRole))
                return;
            if (Flag == Flags.CorpSAG2 && CorporationRole.HangarCanTake2.Is (corporationRole))
                return;
            if (Flag == Flags.CorpSAG3 && CorporationRole.HangarCanTake3.Is (corporationRole))
                return;
            if (Flag == Flags.CorpSAG4 && CorporationRole.HangarCanTake4.Is (corporationRole))
                return;
            if (Flag == Flags.CorpSAG5 && CorporationRole.HangarCanTake5.Is (corporationRole))
                return;
            if (Flag == Flags.CorpSAG6 && CorporationRole.HangarCanTake6.Is (corporationRole))
                return;
            if (Flag == Flags.CorpSAG7 && CorporationRole.HangarCanTake7.Is (corporationRole))
                return;
        }
        else
        {
            if (Flag == Flags.CorpMarket && CorporationRole.Trader.Is (corporationRole))
                return;
            if (Flag == Flags.Hangar && CorporationRole.HangarCanQuery1.Is (corporationRole))
                return;
            if (Flag == Flags.CorpSAG2 && CorporationRole.HangarCanQuery2.Is (corporationRole))
                return;
            if (Flag == Flags.CorpSAG3 && CorporationRole.HangarCanQuery3.Is (corporationRole))
                return;
            if (Flag == Flags.CorpSAG4 && CorporationRole.HangarCanQuery4.Is (corporationRole))
                return;
            if (Flag == Flags.CorpSAG5 && CorporationRole.HangarCanQuery5.Is (corporationRole))
                return;
            if (Flag == Flags.CorpSAG6 && CorporationRole.HangarCanQuery6.Is (corporationRole))
                return;
            if (Flag == Flags.CorpSAG7 && CorporationRole.HangarCanQuery7.Is (corporationRole))
                return;
        }

        throw new MktNotOwner ();
    }

    public bool IsInModuleSlot ()
    {
        return Flag == Flags.HiSlot0 || Flag == Flags.HiSlot1 || Flag == Flags.HiSlot2 ||
               Flag == Flags.HiSlot3 || Flag == Flags.HiSlot4 || Flag == Flags.HiSlot5 ||
               Flag == Flags.HiSlot6 || Flag == Flags.HiSlot7 || Flag == Flags.MedSlot0 ||
               Flag == Flags.MedSlot1 || Flag == Flags.MedSlot2 || Flag == Flags.MedSlot3 ||
               Flag == Flags.MedSlot4 || Flag == Flags.MedSlot5 || Flag == Flags.MedSlot6 ||
               Flag == Flags.MedSlot7 || Flag == Flags.LoSlot0 || Flag == Flags.LoSlot1 ||
               Flag == Flags.LoSlot2 || Flag == Flags.LoSlot3 || Flag == Flags.LoSlot4 ||
               Flag == Flags.LoSlot5 || Flag == Flags.LoSlot6 || Flag == Flags.LoSlot7;
    }

    public bool IsInRigSlot ()
    {
        return Flag == Flags.RigSlot0 || Flag == Flags.RigSlot1 || Flag == Flags.RigSlot2 ||
               Flag == Flags.RigSlot3 || Flag == Flags.RigSlot4 || Flag == Flags.RigSlot5 ||
               Flag == Flags.RigSlot6 || Flag == Flags.RigSlot7;
    }
}
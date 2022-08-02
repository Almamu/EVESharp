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
using System.Globalization;
using System.IO;
using EVESharp.EVE.Dogma.Exception;
using EVESharp.EVE.StaticData.Dogma;
using EVESharp.EVE.StaticData.Inventory;
using EVESharp.PythonTypes.Types.Primitives;

namespace EVESharp.EVE.Inventory.Attributes;

public class Attribute
{
    public enum ItemAttributeValueType
    {
        Integer = 0,
        Double  = 1
    }

    private const    double          TOLERANCE = 0.0001;
    private readonly List <Modifier> mModifiers;
    private          double          mFloat;

    private long mInteger;
    public  bool Dirty { get; set; }
    public  bool New   { get; set; }

    public int                    ID        { get; }
    public ItemAttributeValueType ValueType { get; protected set; }

    public long Integer
    {
        get => this.mInteger;
        set
        {
            // ensure the type is updated to the correct one
            ValueType = ItemAttributeValueType.Integer;

            this.mInteger = value;
            Dirty         = true;
        }
    }

    public double Float
    {
        get => this.mFloat;
        set
        {
            // ensure the type is updated to the correct one
            ValueType = ItemAttributeValueType.Double;

            this.mFloat = value;
            Dirty       = true;
        }
    }

    public Attribute (int id, double value, bool newEntity = false, List <Modifier> modifiers = null)
    {
        ID              = id;
        Float           = value;
        New             = newEntity;
        this.mModifiers = modifiers ?? new List <Modifier> ();
    }

    public Attribute (int id, long value, bool newEntity = false, List <Modifier> modifiers = null)
    {
        ID              = id;
        Integer         = value;
        New             = newEntity;
        this.mModifiers = modifiers ?? new List <Modifier> ();
    }

    public Attribute (AttributeType attribute, double value, bool newEntity = false, List <Modifier> modifiers = null)
        : this (attribute.ID, value, newEntity, modifiers)
    {
    }

    public Attribute (AttributeType attribute, long value, bool newEntity = false, List <Modifier> modifiers = null)
        : this (attribute.ID, value, newEntity, modifiers)
    {
    }

    public Attribute (AttributeType attribute, bool newEntity = false, List <Modifier> modifiers = null)
        : this (attribute.ID, attribute.DefaultValue, newEntity, modifiers)
    {
    }

    public Attribute (AttributeTypes attribute, double value, bool newEntity = false, List <Modifier> modifiers = null)
        : this((int) attribute, value, newEntity, modifiers)
    {
    }
    
    protected bool Equals (Attribute other)
    {
        if (ValueType == ItemAttributeValueType.Integer && other.ValueType == ItemAttributeValueType.Integer)
            return Integer == other.Integer;
        if (ValueType == ItemAttributeValueType.Double && other.ValueType == ItemAttributeValueType.Integer)
            return Math.Abs (Float - other.Integer) < TOLERANCE;
        if (ValueType == ItemAttributeValueType.Integer && other.ValueType == ItemAttributeValueType.Double)
            return Math.Abs (other.Float - Integer) < TOLERANCE;
        if (ValueType == ItemAttributeValueType.Double && other.ValueType == ItemAttributeValueType.Double)
            return Math.Abs (Float - other.Float) < TOLERANCE;

        return false;
    }

    public override bool Equals (object obj)
    {
        if (ReferenceEquals (null, obj)) return false;
        if (ReferenceEquals (this, obj)) return true;
        if (obj.GetType () != this.GetType ()) return false;

        return this.Equals ((Attribute) obj);
    }

    public void AddModifier (Association modificationType, Attribute value)
    {
        this.mModifiers.Add (
            new Modifier
            {
                Modification = modificationType,
                Value        = value
            }
        );
    }

    public void RemoveModifier (Association modificationType, Attribute value)
    {
        this.mModifiers.RemoveAll (x => ReferenceEquals (x.Value, value) && x.Modification == modificationType);
    }

    public Attribute ApplyModifiers ()
    {
        Attribute attribute = this.Clone ();

        // perform pre changes
        foreach (Modifier modifier in this.mModifiers)
            switch (modifier.Modification)
            {
                case Association.PreAssignment:
                    attribute = modifier.Value;
                    break;

                case Association.SkillCheck:
                    throw new DogmaMachineException ("SkillCheck not supported yet");
                case Association.PreDiv:
                    if (attribute != 0)
                        attribute /= modifier.Value;
                    break;

                case Association.PreMul:
                    attribute *= modifier.Value;
                    break;

                case Association.ModAdd:
                    attribute += modifier.Value;
                    break;

                case Association.ModSub:
                    attribute -= modifier.Value;
                    break;

                case Association.AddRate:
                case Association.SubRate:
                    throw new DogmaMachineException ("AddRate/SubRate not supported yet");
            }

        // perform post changes
        foreach (Modifier modifier in this.mModifiers)
            switch (modifier.Modification)
            {
                case Association.PostAssignment:
                    attribute = modifier.Value;
                    break;

                case Association.SkillCheck:
                    throw new DogmaMachineException ("SkillCheck not supported yet");
                case Association.PostDiv:
                    if (attribute != 0)
                        attribute /= modifier.Value;
                    break;

                case Association.PostMul:
                    attribute *= modifier.Value;
                    break;

                case Association.PostPercent:
                    attribute *= 1.0 + modifier.Value / 100.0;
                    break;

                case Association.AddRate:
                case Association.SubRate:
                    throw new DogmaMachineException ("AddRate/SubRate not supported yet");
            }

        return attribute;
    }

    /// <summary>
    /// Performs a clone of the item attribute information
    /// </summary>
    /// <returns></returns>
    public Attribute Clone ()
    {
        switch (ValueType)
        {
            case ItemAttributeValueType.Double:
                return new Attribute (ID, Float, true, this.mModifiers);
            case ItemAttributeValueType.Integer:
                return new Attribute (ID, Integer, true, this.mModifiers);
            default:
                throw new InvalidDataException ();
        }
    }

    public static implicit operator PyDataType (Attribute attribute)
    {
        // when converting the attribute to a primitive value
        // the important thing is to ensure that the value actually has all the modifiers applied
        Attribute final = attribute.ApplyModifiers ();

        return final.ValueType switch
        {
            ItemAttributeValueType.Double  => new PyDecimal (final.Float),
            ItemAttributeValueType.Integer => new PyInteger (final.Integer),
            _                              => throw new InvalidDataException ()
        };
    }

    public static Attribute operator * (Attribute original, double value)
    {
        // clone the attribute but mark it as existant
        Attribute clone = original.Clone ();
        clone.New = false;

        switch (clone.ValueType)
        {
            // based on the types perform the appropiate operation
            case ItemAttributeValueType.Double:
                clone.Float *= value;
                break;

            case ItemAttributeValueType.Integer:
                clone.Float = clone.Integer * value;
                break;

        }

        return clone;
    }

    public static double operator * (double value, Attribute original)
    {
        return original * value;
    }

    public static Attribute operator * (Attribute original, long value)
    {
        // clone the attribute but mark it as existant
        Attribute clone = original.Clone ();
        clone.New = false;

        switch (clone.ValueType)
        {
            // based on the types perform the appropiate operation
            case ItemAttributeValueType.Integer:
                clone.Integer *= value;
                break;

            case ItemAttributeValueType.Double:
                clone.Float *= value;
                break;

        }

        return clone;
    }

    public static Attribute operator * (Attribute original, Attribute value)
    {
        // clone the attribute but mark it as existant
        Attribute clone = original.Clone ();
        clone.New = false;

        switch (value.ValueType)
        {
            // based on the types perform the aproppiate operation
            case ItemAttributeValueType.Integer:
                switch (clone.ValueType)
                {
                    case ItemAttributeValueType.Double:
                        clone.Float *= value.Integer;
                        break;

                    case ItemAttributeValueType.Integer:
                        clone.Integer *= value.Integer;
                        break;

                }
                break;

            case ItemAttributeValueType.Double:
                switch (clone.ValueType)
                {
                    case ItemAttributeValueType.Double:
                        clone.Float *= value.Float;
                        break;

                    case ItemAttributeValueType.Integer:
                        clone.Float = clone.Integer * value.Float;
                        break;

                }
                break;

        }

        return clone;
    }

    public static Attribute operator / (Attribute original, double value)
    {
        // clone the attribute but mark it as existant
        Attribute clone = original.Clone ();
        clone.New = false;

        switch (clone.ValueType)
        {
            // based on the types perform the appropiate operation
            case ItemAttributeValueType.Integer:
                clone.Float = clone.Integer / value;
                break;

            case ItemAttributeValueType.Double:
                clone.Float /= value;
                break;

        }

        return clone;
    }

    public static Attribute operator / (Attribute original, long value)
    {
        // clone the attribute but mark it as existant
        Attribute clone = original.Clone ();
        clone.New = false;

        switch (clone.ValueType)
        {
            // based on the types perform the appropiate operation
            case ItemAttributeValueType.Integer:
                clone.Float = (double) clone.Integer / value;
                break;

            case ItemAttributeValueType.Double:
                clone.Float /= value;
                break;

        }

        return clone;
    }

    public static Attribute operator / (Attribute original, Attribute value)
    {
        // clone the attribute but mark it as existant
        Attribute clone = original.Clone ();
        clone.New = false;

        switch (value.ValueType)
        {
            // based on the types perform the aproppiate operation
            case ItemAttributeValueType.Integer:
                switch (clone.ValueType)
                {
                    case ItemAttributeValueType.Double:
                        clone.Float /= value.Integer;
                        break;

                    case ItemAttributeValueType.Integer:
                        clone.Float = (double) clone.Integer / value.Integer;
                        break;

                }
                break;

            case ItemAttributeValueType.Double:
                switch (clone.ValueType)
                {
                    case ItemAttributeValueType.Double:
                        clone.Float /= value.Float;
                        break;

                    case ItemAttributeValueType.Integer:
                        clone.Float = clone.Integer / value.Float;
                        break;

                }
                break;

        }

        return clone;
    }

    public static Attribute operator + (Attribute original, double value)
    {
        // clone the attribute but mark it as existant
        Attribute clone = original.Clone ();
        clone.New = false;

        switch (clone.ValueType)
        {
            // based on the types perform the appropiate operation
            case ItemAttributeValueType.Double:
                clone.Float += value;
                break;

            case ItemAttributeValueType.Integer:
                clone.Float = clone.Integer + value;
                break;

        }

        return clone;
    }

    public static Attribute operator + (Attribute original, long value)
    {
        // clone the attribute but mark it as existant
        Attribute clone = original.Clone ();
        clone.New = false;

        switch (clone.ValueType)
        {
            // based on the types perform the appropiate operation
            case ItemAttributeValueType.Integer:
                clone.Integer += value;
                break;

            case ItemAttributeValueType.Double:
                clone.Float += value;
                break;

        }

        return clone;
    }

    public static Attribute operator + (Attribute original, Attribute value)
    {
        // clone the attribute but mark it as existant
        Attribute clone = original.Clone ();
        clone.New = false;

        switch (value.ValueType)
        {
            // based on the types perform the aproppiate operation
            case ItemAttributeValueType.Integer:
                switch (clone.ValueType)
                {
                    case ItemAttributeValueType.Double:
                        clone.Float += value.Integer;
                        break;

                    case ItemAttributeValueType.Integer:
                        clone.Integer += value.Integer;
                        break;

                }
                break;

            case ItemAttributeValueType.Double:
                switch (clone.ValueType)
                {
                    case ItemAttributeValueType.Double:
                        clone.Float += value.Float;
                        break;

                    case ItemAttributeValueType.Integer:
                        clone.Float = clone.Integer + value.Float;
                        break;

                }
                break;

        }

        return clone;
    }

    public static Attribute operator - (Attribute original, double value)
    {
        // clone the attribute but mark it as existant
        Attribute clone = original.Clone ();
        clone.New = false;

        switch (clone.ValueType)
        {
            // based on the types perform the appropiate operation
            case ItemAttributeValueType.Double:
                clone.Float -= value;
                break;

            case ItemAttributeValueType.Integer:
                clone.Float = clone.Integer - value;
                break;

        }

        return clone;
    }

    public static Attribute operator - (Attribute original, long value)
    {
        // clone the attribute but mark it as existant
        Attribute clone = original.Clone ();
        clone.New = false;

        switch (clone.ValueType)
        {
            // based on the types perform the appropiate operation
            case ItemAttributeValueType.Integer:
                clone.Integer -= value;
                break;

            case ItemAttributeValueType.Double:
                clone.Float -= value;
                break;

        }

        return clone;
    }

    public static Attribute operator - (Attribute original, Attribute value)
    {
        // clone the attribute but mark it as existant
        Attribute clone = original.Clone ();
        clone.New = false;

        switch (value.ValueType)
        {
            // based on the types perform the appropiate operation
            case ItemAttributeValueType.Integer:
                switch (clone.ValueType)
                {
                    case ItemAttributeValueType.Double:
                        clone.Float -= value.Integer;
                        break;

                    case ItemAttributeValueType.Integer:
                        clone.Integer -= value.Integer;
                        break;

                }
                break;

            case ItemAttributeValueType.Double:
                switch (clone.ValueType)
                {
                    case ItemAttributeValueType.Double:
                        clone.Float -= value.Float;
                        break;

                    case ItemAttributeValueType.Integer:
                        clone.Float = clone.Integer - value.Float;
                        break;

                }
                break;

        }

        return clone;
    }

    public static bool operator == (Attribute attrib, int value)
    {
        if (ReferenceEquals (null, attrib)) return false;

        switch (attrib.ValueType)
        {
            case ItemAttributeValueType.Integer:
                return attrib.Integer == value;
            default:
                return Math.Abs (attrib.Float - value) < TOLERANCE;
        }
    }

    public static bool operator != (Attribute attrib, int value)
    {
        return !(attrib == value);
    }

    public static implicit operator double (Attribute attrib)
    {
        // when converting the attribute to a primitive value
        // the important thing is to ensure that the value actually has all the modifiers applied
        Attribute final = attrib.ApplyModifiers ();

        switch (final.ValueType)
        {
            case ItemAttributeValueType.Double:
                return final.Float;
            default:
            case ItemAttributeValueType.Integer:
                return final.Integer;
        }
    }

    public static implicit operator Attribute (AttributeType info)
    {
        return new Attribute (info);
    }

    public override string ToString ()
    {
        if (ValueType == ItemAttributeValueType.Double)
            return Float.ToString (CultureInfo.InvariantCulture);
        if (ValueType == ItemAttributeValueType.Integer)
            return Integer.ToString ();

        // this should never happen tho
        return "Unknown";
    }

    public override int GetHashCode ()
    {
        return ID;
    }
}
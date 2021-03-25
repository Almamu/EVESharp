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
using Common.Database;
using Node.Dogma.Exception;
using Node.Inventory.Items.Dogma;
using PythonTypes.Types.Primitives;

namespace Node.Inventory.Items.Attributes
{
    public class ItemAttribute : DatabaseEntity
    {
        // TODO: Create a PyNumber class to handle both integer and double values to easily access these and send them to the client?
        public enum ItemAttributeValueType
        {
            Integer = 0,
            Double = 1
        }

        private long mInteger;
        private double mFloat;
        private AttributeInfo mInfo;
        private readonly List<AttributeModifier> mModifiers;

        public AttributeInfo Info => this.mInfo;
        public ItemAttributeValueType ValueType { get; protected set; }

        public long Integer
        {
            get => this.mInteger;
            set
            {
                // ensure the type is updated to the correct one
                this.ValueType = ItemAttributeValueType.Integer;
                
                this.mInteger = value;
                this.Dirty = true;
            }
        }

        public double Float
        {
            get => this.mFloat;
            set
            {
                // ensure the type is updated to the correct one
                this.ValueType = ItemAttributeValueType.Double;
                
                this.mFloat = value;
                this.Dirty = true;
            }
        }

        public ItemAttribute(AttributeInfo attribute, double value, bool newEntity = false, List<AttributeModifier> modifiers = null)
        {
            this.mInfo = attribute;
            this.mFloat = value;
            this.ValueType = ItemAttributeValueType.Double;
            this.New = newEntity;
            this.mModifiers = modifiers ?? new List<AttributeModifier>();
        }

        public ItemAttribute(AttributeInfo attribute, long value, bool newEntity = false, List<AttributeModifier> modifiers = null)
        {
            this.mInfo = attribute;
            this.mInteger = value;
            this.ValueType = ItemAttributeValueType.Integer;
            this.New = newEntity;
            this.mModifiers = modifiers ?? new List<AttributeModifier>();
        }

        public void AddModifier(Association modificationType, ItemAttribute value)
        {
            this.mModifiers.Add(new AttributeModifier()
                {
                    Modification = modificationType,
                    Value = value
                }
            );
        }

        public void RemoveModifier(Association modificationType, ItemAttribute value)
        {
            this.mModifiers.RemoveAll(x => ReferenceEquals(x.Value, value) == true && x.Modification == modificationType);
        }

        public ItemAttribute ApplyModififers()
        {
            // these objects should be shortlived
            // so better clone them and do not modify the original attribute
            ItemAttribute attribute = this.Clone();
            
            // perform pre changes
            foreach (AttributeModifier modifier in this.mModifiers)
            {
                switch (modifier.Modification)
                {
                    case Association.PreAssignment:
                        attribute = modifier.Value;
                        break;
                    case Association.SkillCheck:
                        throw new DogmaMachineException("SkillCheck not supported yet");
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
                        throw new DogmaMachineException("AddRate/SubRate not supported yet");
                }
            }
            
            // perform post changes
            foreach (AttributeModifier modifier in this.mModifiers)
            {
                switch (modifier.Modification)
                {
                    case Association.PostAssignment:
                        attribute = modifier.Value;
                        break;
                    case Association.SkillCheck:
                        throw new DogmaMachineException("SkillCheck not supported yet");
                    case Association.PostDiv:
                        if (attribute != 0)
                            attribute /= modifier.Value;
                        break;
                    case Association.PostMul:
                        attribute *= modifier.Value;
                        break;
                    case Association.PostPercent:
                        attribute *= (1.0 + (modifier.Value / 100.0));
                        break;
                    case Association.AddRate:
                    case Association.SubRate:
                        throw new DogmaMachineException("AddRate/SubRate not supported yet");
                }
            }

            return attribute;
        }

        /// <summary>
        /// Performs a clone of the item attribute information
        /// </summary>
        /// <returns></returns>
        public ItemAttribute Clone()
        {
            switch (this.ValueType)
            {
                case ItemAttributeValueType.Double:
                    return new ItemAttribute(this.Info, this.Float, true);
                case ItemAttributeValueType.Integer:
                    return new ItemAttribute(this.Info, this.Integer, true);
                default:
                    throw new InvalidDataException();
            }
        }

        protected override void SaveToDB()
        {
            // item attributes cannot be saved by themselves
            // only AttributeList have enough information to perform that save
            // so use that class instead
            throw new System.NotImplementedException();
        }
        
        public static implicit operator PyDataType(ItemAttribute attribute)
        {
            // when converting the attribute to a primitive value
            // the important thing is to ensure that the value actually has all the modifiers applied
            ItemAttribute final = attribute.ApplyModififers();

            switch (final.ValueType)
            {
                case ItemAttributeValueType.Double:
                    return new PyDecimal(final.Float);
                case ItemAttributeValueType.Integer:
                    return new PyInteger(final.Integer);
                default:
                    throw new InvalidDataException();
            }
        }

        public static ItemAttribute operator *(ItemAttribute original, double value)
        {
            // clone the attribute but mark it as existant
            ItemAttribute clone = original.Clone();
            clone.New = false;
            
            // based on the types perform the appropiate operation
            if (clone.ValueType == ItemAttributeValueType.Double)
                clone.Float *= value;
            else if (clone.ValueType == ItemAttributeValueType.Integer)
                clone.Float = clone.Integer * value;

            return clone;
        }

        public static ItemAttribute operator *(double value, ItemAttribute original)
        {
            return original * value;
        }

        public static ItemAttribute operator *(ItemAttribute original, long value)
        {
            // clone the attribute but mark it as existant
            ItemAttribute clone = original.Clone();
            clone.New = false;
            
            // based on the types perform the appropiate operation
            if (clone.ValueType == ItemAttributeValueType.Integer)
                clone.Integer *= value;
            else if (clone.ValueType == ItemAttributeValueType.Double)
                clone.Float *= value;

            return clone;
        }

        public static ItemAttribute operator *(int value, ItemAttribute original)
        {
            return original * value;
        }

        public static ItemAttribute operator *(ItemAttribute original, ItemAttribute value)
        {
            // clone the attribute but mark it as existant
            ItemAttribute clone = original.Clone();
            clone.New = false;
            
            // based on the types perform the appropiate operation
            if (value.ValueType == ItemAttributeValueType.Integer)
                clone *= value.Integer;
            else if (value.ValueType == ItemAttributeValueType.Double)
                clone *= value.Float;

            return clone;
        }
        
        public static ItemAttribute operator /(ItemAttribute original, double value)
        {
            // clone the attribute but mark it as existant
            ItemAttribute clone = original.Clone();
            clone.New = false;
            
            // based on the types perform the appropiate operation
            if (clone.ValueType == ItemAttributeValueType.Double)
                clone.Float /= value;
            else if (clone.ValueType == ItemAttributeValueType.Integer)
                clone.Float = clone.Integer / value;

            return clone;
        }

        public static ItemAttribute operator /(ItemAttribute original, long value)
        {
            // clone the attribute but mark it as existant
            ItemAttribute clone = original.Clone();
            clone.New = false;
            
            // based on the types perform the appropiate operation
            if (clone.ValueType == ItemAttributeValueType.Integer)
                clone.Integer /= value;
            else if (clone.ValueType == ItemAttributeValueType.Double)
                clone.Float /= value;

            return clone;
        }

        public static ItemAttribute operator /(ItemAttribute original, ItemAttribute value)
        {
            // clone the attribute but mark it as existant
            ItemAttribute clone = original.Clone();
            clone.New = false;
            
            // based on the types perform the appropiate operation
            if (value.ValueType == ItemAttributeValueType.Integer)
                clone /= value.Integer;
            else if (value.ValueType == ItemAttributeValueType.Double)
                clone /= value.Float;

            return clone;
        }
        public static ItemAttribute operator +(ItemAttribute original, double value)
        {
            // clone the attribute but mark it as existant
            ItemAttribute clone = original.Clone();
            clone.New = false;
            
            // based on the types perform the appropiate operation
            if (clone.ValueType == ItemAttributeValueType.Double)
                clone.Float += value;
            else if (clone.ValueType == ItemAttributeValueType.Integer)
                clone.Float = clone.Integer + value;

            return clone;
        }

        public static ItemAttribute operator +(ItemAttribute original, long value)
        {
            // clone the attribute but mark it as existant
            ItemAttribute clone = original.Clone();
            clone.New = false;
            
            // based on the types perform the appropiate operation
            if (clone.ValueType == ItemAttributeValueType.Integer)
                clone.Integer += value;
            else if (clone.ValueType == ItemAttributeValueType.Double)
                clone.Float += value;

            return clone;
        }

        public static ItemAttribute operator +(ItemAttribute original, ItemAttribute value)
        {
            // clone the attribute but mark it as existant
            ItemAttribute clone = original.Clone();
            clone.New = false;
            
            // based on the types perform the appropiate operation
            if (value.ValueType == ItemAttributeValueType.Integer)
                clone += value.Integer;
            else if (value.ValueType == ItemAttributeValueType.Double)
                clone += value.Float;

            return clone;
        }
        public static ItemAttribute operator -(ItemAttribute original, double value)
        {
            // clone the attribute but mark it as existant
            ItemAttribute clone = original.Clone();
            clone.New = false;
            
            // based on the types perform the appropiate operation
            if (clone.ValueType == ItemAttributeValueType.Double)
                clone.Float -= value;
            else if (clone.ValueType == ItemAttributeValueType.Integer)
                clone.Float = clone.Integer - value;

            return clone;
        }

        public static ItemAttribute operator -(ItemAttribute original, long value)
        {
            // clone the attribute but mark it as existant
            ItemAttribute clone = original.Clone();
            clone.New = false;
            
            // based on the types perform the appropiate operation
            if (clone.ValueType == ItemAttributeValueType.Integer)
                clone.Integer -= value;
            else if (clone.ValueType == ItemAttributeValueType.Double)
                clone.Float -= value;

            return clone;
        }

        public static ItemAttribute operator -(ItemAttribute original, ItemAttribute value)
        {
            // clone the attribute but mark it as existant
            ItemAttribute clone = original.Clone();
            clone.New = false;
            
            // based on the types perform the appropiate operation
            if (value.ValueType == ItemAttributeValueType.Integer)
                clone -= value.Integer;
            else if (value.ValueType == ItemAttributeValueType.Double)
                clone -= value.Float;

            return clone;
        }

        public static bool operator ==(ItemAttribute attrib, int value)
        {
            switch (attrib.ValueType)
            {
                case ItemAttributeValueType.Integer:
                    return attrib.Integer == value;
                default:
                    return attrib.Float == value;
            }
        }

        public static bool operator !=(ItemAttribute attrib, int value)
        {
            return !(attrib == value);
        }

        public static implicit operator double(ItemAttribute attrib)
        {
            // when converting the attribute to a primitive value
            // the important thing is to ensure that the value actually has all the modifiers applied
            ItemAttribute final = attrib.ApplyModififers();
            
            switch (final.ValueType)
            {
                case ItemAttributeValueType.Double:
                    return final.Float;
                default:
                    return final.Integer;
            }
        }

        public override string ToString()
        {
            if (this.ValueType == ItemAttributeValueType.Double)
                return this.Float.ToString(CultureInfo.InvariantCulture);
            if (this.ValueType == ItemAttributeValueType.Integer)
                return this.Integer.ToString();

            // this should never happen tho
            return "Unknown";
        }

        public override int GetHashCode()
        {
            return this.Info.ID;
        }
    }
}
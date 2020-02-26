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

using System.IO;
using System.Xml.Schema;
using Common.Database;

namespace Node.Inventory.Items.Attributes
{
    public class ItemAttribute : DatabaseEntity
    {
        // TODO: Create a PyNumber class to handle both integer and double values to easily access these and send them to the client?
        public enum ItemAttributeValueType
        {
            Integer = 0,
            Double = 1
        };

        private int mInteger;
        private double mFloat;
        
        public AttributeInfo Info { get; }
        public ItemAttributeValueType ValueType { get; }

        public int Integer
        {
            get => this.mInteger;
            set
            {
                this.mInteger = value;
                this.Dirty = true;
            }
        }

        public double Float
        {
            get => this.mFloat;
            set
            {
                this.mFloat = value;
                this.Dirty = true;
            }
        }

        public ItemAttribute(AttributeInfo attribute, double value, bool newEntity = false)
        {
            this.Info = attribute;
            this.mFloat = value;
            this.ValueType = ItemAttributeValueType.Double;
            this.New = newEntity;
        }

        public ItemAttribute(AttributeInfo attribute, int value, bool newEntity = false)
        {
            this.Info = attribute;
            this.mInteger = value;
            this.ValueType = ItemAttributeValueType.Integer;
            this.New = newEntity;
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
                    throw new InvalidDataException("");
            }
        }

        protected override void SaveToDB()
        {
            // item attributes cannot be saved by themselves
            // only AttributeList have enough information to perform that save
            // so use that class instead
            throw new System.NotImplementedException();
        }
    }
}
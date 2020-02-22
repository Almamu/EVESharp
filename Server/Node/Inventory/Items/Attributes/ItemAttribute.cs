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

namespace Node.Inventory
{
    public class ItemAttribute
    {
        // TODO: Create a PyNumber class to handle both integer and double values to easily access these and send them to the client?
        public enum ItemAttributeValueType
        {
            Integer = 0,
            Double = 1
        };
        
        public AttributeInfo AttributeInfo { get; }
        public ItemAttributeValueType ValueType { get; }
        public int Integer { get; set; }
        public double Float { get; set; }

        public ItemAttribute(AttributeInfo attribute, double value)
        {
            this.AttributeInfo = attribute;
            this.Float = value;
            this.ValueType = ItemAttributeValueType.Double;
        }

        public ItemAttribute(AttributeInfo attribute, int value)
        {
            this.AttributeInfo = attribute;
            this.Integer = value;
            this.ValueType = ItemAttributeValueType.Integer;
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
                    return new ItemAttribute(this.AttributeInfo, this.Float);
                case ItemAttributeValueType.Integer:
                    return new ItemAttribute(this.AttributeInfo, this.Integer);
                default:
                    throw new InvalidDataException("");
            }
        }
    }
}
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using EVESharp.Database;

namespace EVESharp.Inventory
{
    public class Attributes
    {
        public Attributes()
        {
            
        }

        public void LoadAttributes(int itemID, int typeID)
        {
            attributes = ItemDB.GetAttributesForItem(itemID);

            // Add the default attributes
            Dictionary<string, ItemAttribute> def = ItemDB.GetDefaultAttributesForType(typeID);

            foreach (KeyValuePair<string, ItemAttribute> pair in def)
            {
                if (attributes.ContainsKey(pair.Key) == false)
                {
                    attributes.Add(pair.Key, pair.Value);
                }
            }
        }

        public int GetInt(string attribute)
        {
            if (attributes.ContainsKey(attribute) == true)
            {
                return attributes[attribute].intValue;
            }

            return 0;
        }

        public float GetFloat(string attribute)
        {
            if (attributes.ContainsKey(attribute) == true)
            {
                return attributes[attribute].floatValue;
            }

            return 0.0f;
        }

        public void Set(string attribute, int value)
        {
            if (attributes.ContainsKey(attribute) == true)
            {
                attributes[attribute].intValue = value;
            }
            else
            {
                ItemAttribute attrib = new ItemAttribute();

                attrib.attributeID = ItemDB.GetAttributeIDForName(attribute);
                attrib.intValue = value;

                attributes.Add(attribute, attrib);
            }
        }

        public void Set(string attribute, float value)
        {
            if (attributes.ContainsKey(attribute) == true)
            {
                attributes[attribute].floatValue = value;
            }
            else
            {
                ItemAttribute attrib = new ItemAttribute();

                attrib.attributeID = ItemDB.GetAttributeIDForName(attribute);
                attrib.floatValue = value;

                attributes.Add(attribute, attrib);
            }
        }

        Dictionary<string, ItemAttribute> attributes = new Dictionary<string, ItemAttribute>();
    }
}

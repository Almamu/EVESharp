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

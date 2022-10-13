using System.Collections;
using System.Collections.Generic;
using EVESharp.Database.Inventory.Types;
using EVESharp.Types.Collections;

namespace EVESharp.EVE.Data.Inventory.Attributes;

public class AttributeList : IEnumerable
{
    private readonly Dictionary <int, Attribute> mDefaultAttributes;
    private readonly Dictionary <int, Attribute> mItemAttributes;

    public Attribute this [int index]
    {
        get
        {
            if (this.mItemAttributes.TryGetValue (index, out Attribute attrib))
                return attrib;
            if (this.mDefaultAttributes.TryGetValue (index, out Attribute defAttrib))
                return this.mItemAttributes [index] = defAttrib;

            // TODO: GET A GOOD DEFAULT VALUE FOR IT, ALTHOUGH ON MOST SITUATIONS THIS MEANS THE ATTRIBUTE IS GOING TO BE UPDATED MANUALLY EITHER WAY
            return this.mItemAttributes [index] = new Attribute (index, 0);
        }

        set => this.mItemAttributes [index] = value;
    }

    public Attribute this [long index]
    {
        get => this [(int) index];
        set => this [(int) index] = value;
    }

    public Attribute this [AttributeTypes index]
    {
        get => this [(int) index];
        set => this [(int) index] = value;
    }

    public AttributeList (Type type, Dictionary <int, Attribute> attributes)
    {
        // load the default attributes list
        this.mDefaultAttributes = type.Attributes;
        // load item attributes
        this.mItemAttributes = attributes;
    }

    public IEnumerator GetEnumerator ()
    {
        return this.mItemAttributes.GetEnumerator ();
    }

    public bool TryGetAttribute (AttributeTypes index, out Attribute attrib)
    {
        return this.mItemAttributes.TryGetValue ((int) index, out attrib) || this.mDefaultAttributes.TryGetValue ((int) index, out attrib);
    }

    public bool AttributeExists (Attribute attribute)
    {
        return this.AttributeExists (attribute.ID);
    }

    public bool AttributeExists (int attributeID)
    {
        return this.mItemAttributes.ContainsKey (attributeID) || this.mDefaultAttributes.ContainsKey (attributeID);
    }

    public bool AttributeExists (AttributeTypes attributeID)
    {
        return this.AttributeExists ((int) attributeID);
    }

    public void MergeFrom (AttributeList list)
    {
        foreach (Attribute attrib in list)
            this [attrib.ID] = attrib.Clone ();
    }

    public void MergeInto (AttributeList list)
    {
        foreach (Attribute attrib in this)
            list [attrib.ID] = attrib.Clone ();
    }

    public static implicit operator PyDictionary (AttributeList list)
    {
        PyDictionary result = new PyDictionary ();

        foreach ((int attribID, Attribute attrib) in list.mDefaultAttributes)
            result [attribID] = attrib;

        foreach ((int attribID, Attribute attrib) in list.mItemAttributes)
            result [attribID] = attrib;

        return result;
    }
}
using System;
using System.Collections;
using System.Collections.Generic;

namespace Node.Inventory.Items.Attributes
{
    public class AttributeList : IEnumerable
    {
        private ItemFactory mItemFactory = null;

        private readonly Dictionary<int, ItemAttribute> mDefaultAttributes = null;
        private readonly Dictionary<int, ItemAttribute> mItemAttributes = null;
        public AttributeList(ItemFactory factory, ItemType type, Dictionary<int, ItemAttribute> attributes)
        {
            this.mItemFactory = factory;
            
            // load the default attributes list
            this.mDefaultAttributes = type.Attributes;
            // load item attributes
            this.mItemAttributes = attributes;
        }

        public ItemAttribute this[int index]
        {
            get
            {
                if (this.mItemAttributes.ContainsKey(index) == false)
                {
                    // clone the default attribute to be used in this list
                    if (this.mDefaultAttributes.ContainsKey(index) == false)
                        throw new IndexOutOfRangeException();
                    
                    this.mItemAttributes[index] = this.mDefaultAttributes[index].Clone();
                }

                return this.mItemAttributes[index];
            }

            set
            {
                if (this.mItemAttributes.ContainsKey(index) == false)
                    this.mItemAttributes.Add(index, value);
                else
                    this.mItemAttributes[index] = value;
            }
        }

        public ItemAttribute this[AttributeEnum index]
        {
            get => this[(int) index];
            set => this[(int) index] = value;
        }

        public bool AttributeExists(ItemAttribute attribute)
        {
            return this.AttributeExists(attribute.Info.ID);
        }

        protected bool AttributeExists(int attributeID)
        {
            return this.mItemAttributes.ContainsKey(attributeID) || this.mDefaultAttributes.ContainsKey(attributeID);
        }

        public void Persist(ItemEntity item)
        {
            this.mItemFactory.ItemDB.PersistAttributeList(item, this);
        }
        
        public IEnumerator GetEnumerator()
        {
            return this.mItemAttributes.GetEnumerator();
        }

        public void MergeFrom(AttributeList list)
        {
            foreach (ItemAttribute attrib in list)
                this[attrib.Info.ID] = attrib.Clone();
        }

        public void MergeInto(AttributeList list)
        {
            foreach (ItemAttribute attrib in this)
                list[attrib.Info.ID] = attrib.Clone();
        }
    }
}
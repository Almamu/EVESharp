using System.Collections;
using System.Collections.Generic;

namespace Node.Inventory
{
    public class AttributeList : IEnumerable
    {
        private ItemFactory mItemFactory = null;
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
                    this.mItemAttributes[index] = this.mDefaultAttributes[index].Clone();
                }

                return this.mItemAttributes[index];
            }
        }

        public void Persist(Entity item)
        {
            this.mItemFactory.ItemDB.PersistAttributeList(item, this);
        }

        private readonly Dictionary<int, ItemAttribute> mDefaultAttributes = null;
        private readonly Dictionary<int, ItemAttribute> mItemAttributes = null;
        
        public IEnumerator GetEnumerator()
        {
            return this.mItemAttributes.GetEnumerator();
        }
    }
}
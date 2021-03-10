using System.Collections.Generic;
using Node.Inventory.Items;
using PythonTypes.Types.Complex;
using PythonTypes.Types.Primitives;

namespace Node.Inventory.Notifications
{
    public class OnItemChange : PyMultiEventEntry
    {
        public ItemEntity Item { get; }
        public Dictionary<ItemChange, PyDataType> Changes { get; } = new Dictionary<ItemChange, PyDataType>();
        
        /// <summary>
        /// The notification name
        /// </summary>
        private const string NOTIFICATION_NAME = "OnItemChange";

        public OnItemChange(ItemEntity item) : base(NOTIFICATION_NAME)
        {
            this.Item = item;
        }

        public void AddChange(ItemChange change, PyDataType oldValue)
        {
            this.Changes[change] = oldValue;
        }

        /// <summary>
        /// Builds the correct PyDictionary for the changes described by this notification
        /// </summary>
        /// <returns></returns>
        private PyDictionary BuildChangeDictionary()
        {
            PyDictionary result = new PyDictionary();

            foreach (KeyValuePair<ItemChange, PyDataType> pair in this.Changes)
                result[(int) pair.Key] = pair.Value;

            return result;
        }

        protected override List<PyDataType> GetElements()
        {
            return new List<PyDataType>()
            {
                this.Item.GetEntityRow(),
                this.BuildChangeDictionary()
            };
        }
    }
}
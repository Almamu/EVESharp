using System;

namespace Node.Inventory.Exceptions
{
    public class ItemNotLoadedException : Exception
    {
        public ItemNotLoadedException(int itemID, string extraInfo = "") : base(
            $"The given item ({itemID}) is not loaded by this ItemManager: {extraInfo}")
        {
           
        }
    }
}
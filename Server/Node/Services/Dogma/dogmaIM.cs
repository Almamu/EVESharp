using System;
using Node.Inventory.Items.Types;
using Org.BouncyCastle.Crypto.Tls;
using PythonTypes.Types.Complex;
using PythonTypes.Types.Exceptions;
using PythonTypes.Types.Primitives;

namespace Node.Services.Dogma
{
    public class dogmaIM : BoundService
    {
        private int mObjectID;
        
        public dogmaIM(ServiceManager manager) : base(manager)
        {
        }

        private dogmaIM(ServiceManager manager, int objectID) : base(manager)
        {
            this.mObjectID = objectID;
        }

        protected override Service CreateBoundInstance(PyTuple objectData)
        {
            return new dogmaIM(this.ServiceManager, objectData[0] as PyInteger);
        }

        public PyDataType ShipGetInfo(PyDictionary namedPayload, Client client)
        {
            if (client.ShipID == null)
                throw new CustomError($"The character is not aboard any ship");
            
            Ship ship = this.ServiceManager.Container.ItemFactory.ItemManager.LoadItem((int) client.ShipID) as Ship;

            if (ship == null)
                throw new CustomError($"Cannot get information for ship {client.ShipID}");
            if (ship.OwnerID != client.CharacterID)
                throw new CustomError("The ship you're trying to get info off does not belong to you");
            
            PyItemInfo itemInfo = new PyItemInfo();
            
            itemInfo.AddRow(
	            ship.ID, ship.GetEntityRow(), ship.GetEffects (), ship.Attributes, DateTime.UtcNow.ToFileTime()
	        );

            return itemInfo;
        }
    }
}
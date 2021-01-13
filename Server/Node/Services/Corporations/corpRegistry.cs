using Common.Database;
using Common.Logging;
using Node.Database;
using Node.Inventory;
using Node.Inventory.Items.Types;
using PythonTypes.Types.Exceptions;
using PythonTypes.Types.Primitives;
using SimpleInjector;

namespace Node.Services.Corporations
{
    public class corpRegistry : BoundService
    {
        private Corporation mCorporation = null;
        private int mIsMaster = 0;

        public Corporation Corporation => this.mCorporation;
        public int IsMaster => this.mIsMaster;

        private CorporationDB DB { get; }
        private ItemManager ItemManager { get; }
        public corpRegistry(CorporationDB db, ItemManager itemManager, BoundServiceManager manager, Logger logger) : base(manager, logger)
        {
            this.DB = db;
            this.ItemManager = itemManager;
        }

        protected corpRegistry(CorporationDB db, ItemManager itemManager, Corporation corp, int isMaster, BoundServiceManager manager, Logger logger) : base (manager, logger)
        {
            this.DB = db;
            this.ItemManager = itemManager;
            this.mCorporation = corp;
            this.mIsMaster = isMaster;
        }

        protected override BoundService CreateBoundInstance(PyDataType objectData)
        {
            PyTuple data = objectData as PyTuple;
            
            /*
             * objectData[0] => corporationID
             * objectData[1] => isMaster
             */
            Corporation corp = this.ItemManager.LoadItem(data[0] as PyInteger) as Corporation;
            
            return new corpRegistry(this.DB, this.ItemManager, corp, data[1] as PyInteger, this.BoundServiceManager, this.Log.Logger);
        }

        public PyDataType GetEveOwners(PyDictionary namedPayload, Client client)
        {
            // this call seems to be getting all the members of the given corporationID
            return this.DB.GetEveOwners(this.Corporation.ID);
        }

        public PyDataType GetCorporation(PyDictionary namedPayload, Client client)
        {
            return this.Corporation.GetCorporationInfoRow();
        }

        public PyDataType GetSharesByShareholder(PyBool corpShares, PyDictionary namedPayload, Client client)
        {
            if (client.CharacterID == null)
                throw new UserError("NoCharacterSelected");
            
            return this.DB.GetSharesByShareholder((int) client.CharacterID);
        }
    }
}
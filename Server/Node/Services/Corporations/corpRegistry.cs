using Common.Database;
using Node.Database;
using Node.Inventory.Items.Types;
using PythonTypes.Types.Exceptions;
using PythonTypes.Types.Primitives;

namespace Node.Services.Corporations
{
    public class corpRegistry : BoundService
    {
        private Corporation mCorporation = null;
        private int mIsMaster = 0;

        public Corporation Corporation => this.mCorporation;
        public int IsMaster => this.mIsMaster;

        private CorporationDB mDB = null;
        public corpRegistry(DatabaseConnection db, ServiceManager manager) : base(manager)
        {
            this.mDB = new CorporationDB(db);
        }

        public corpRegistry(CorporationDB db, Corporation corp, int isMaster, ServiceManager manager) : base (manager)
        {
            this.mDB = db;
            this.mCorporation = corp;
            this.mIsMaster = isMaster;
        }

        protected override Service CreateBoundInstance(PyDataType objectData)
        {
            PyTuple data = objectData as PyTuple;
            
            /*
             * objectData[0] => corporationID
             * objectData[1] => isMaster
             */
            Corporation corp =
                this.ServiceManager.Container.ItemFactory.ItemManager.LoadItem(data[0] as PyInteger) as Corporation;
            
            return new corpRegistry(this.mDB, corp, data[1] as PyInteger, this.ServiceManager);
        }

        public PyDataType GetEveOwners(PyDictionary namedPayload, Client client)
        {
            // this call seems to be getting all the members of the given corporationID
            return this.mDB.GetEveOwners(this.Corporation.ID);
        }

        public PyDataType GetCorporation(PyDictionary namedPayload, Client client)
        {
            return this.Corporation.GetCorporationInfoRow();
        }

        public PyDataType GetSharesByShareholder(PyBool corpShares, PyDictionary namedPayload, Client client)
        {
            if (client.CharacterID == null)
                throw new UserError("NoCharacterSelected");
            
            return this.mDB.GetSharesByShareholder((int) client.CharacterID);
        }
    }
}
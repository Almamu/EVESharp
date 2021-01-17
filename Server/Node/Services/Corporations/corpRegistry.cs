using System.Collections.Generic;
using Common.Database;
using Common.Logging;
using Node.Database;
using Node.Inventory;
using Node.Inventory.Items.Types;
using PythonTypes.Types.Database;
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
        public corpRegistry(CorporationDB db, ItemManager itemManager, BoundServiceManager manager) : base(manager)
        {
            this.DB = db;
            this.ItemManager = itemManager;
        }

        protected corpRegistry(CorporationDB db, ItemManager itemManager, Corporation corp, int isMaster, BoundServiceManager manager) : base (manager)
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
            
            return new corpRegistry(this.DB, this.ItemManager, corp, data[1] as PyInteger, this.BoundServiceManager);
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

        public PyDataType GetMember(PyInteger memberID, PyDictionary namedPayload, Client client)
        {
            return this.DB.GetMember(memberID, client.CorporationID);
        }

        public PyDataType GetMembers(PyDictionary namedPayload, Client client)
        {
            // generate the sparse rowset
            SparseRowsetHeader rowsetHeader = this.DB.GetMembersSparseRowset(client.CorporationID);

            PyDictionary dict = new PyDictionary
            {
                ["realRowCount"] = rowsetHeader.Count
            };
            
            // create a service for handling it's calls
            MembersSparseRowsetService svc =
                new MembersSparseRowsetService(this.Corporation, this.DB, rowsetHeader, this.BoundServiceManager);

            rowsetHeader.BoundObjectIdentifier = svc.MachoBindObject(dict, client);
            
            // finally return the data
            return rowsetHeader;
        }

        public PyDataType GetOffices(PyDictionary namedPayload, Client client)
        {
            // generate the sparse rowset
            SparseRowsetHeader rowsetHeader = this.DB.GetOfficesSparseRowset(client.CorporationID);

            PyDictionary dict = new PyDictionary
            {
                ["realRowCount"] = rowsetHeader.Count
            };
            
            // create a service for handling it's calls
            OfficesSparseRowsetService svc =
                new OfficesSparseRowsetService(this.Corporation, this.DB, rowsetHeader, this.BoundServiceManager);

            rowsetHeader.BoundObjectIdentifier = svc.MachoBindObject(dict, client);
            
            // finally return the data
            return rowsetHeader;
        }

        public PyDataType GetRoleGroups(PyDictionary namedPayload, Client client)
        {
            return this.DB.GetRoleGroups();
        }

        public PyDataType GetRoles(PyDictionary namedPayload, Client client)
        {
            return this.DB.GetRoles();
        }

        public PyDataType GetDivisions(PyDictionary namedPayload, Client client)
        {
            return this.DB.GetDivisions();
        }

        public PyDataType GetTitles(PyDictionary namedPayload, Client client)
        {
            // check if the corp is NPC and return placeholder data from the crpTitlesTemplate
            if (ItemManager.IsNPCCorporationID(client.CorporationID) == true)
            {
                return this.DB.GetTitlesTemplate();
            }
            else
            {
                return this.DB.GetTitles(client.CorporationID);                
            }
        }

        public PyDataType GetMemberTrackingInfo(PyInteger characterID, PyDictionary namedPayload, Client client)
        {
            // TODO: RETURN FULL TRACKING INFO, ONLY DIRECTORS ARE ALLOWED TO DO SO!
            return null;
        }

        public PyDataType GetMemberTrackingInfoSimple(PyDictionary namedPayload, Client client)
        {
            return this.DB.GetMemberTrackingInfoSimple(client.CorporationID);
        }

        public PyDataType GetInfoWindowDataForChar(PyInteger characterID, PyDictionary namedPayload, Client client)
        {
            int titleMask = this.DB.GetTitleMaskForCharacter(characterID, client.CorporationID);
            Dictionary<int, string> titles = this.DB.GetTitlesNames(client.CorporationID);
            PyDictionary dictForKeyVal = new PyDictionary();

            int number = 0;

            foreach (KeyValuePair<int, string> title in titles)
                dictForKeyVal["title" + (++number)] = title.Value;
            
            // we're supposed to be from the same corp, so add the extra information manually
            // TODO: TEST WITH USERS FROM OTHER CORPS
            dictForKeyVal["corpID"] = client.CorporationID;
            dictForKeyVal["allianceID"] = client.AllianceID;
            dictForKeyVal["title"] = "TITLE HERE";

            return KeyVal.FromDictionary(dictForKeyVal);
        }
    }
}
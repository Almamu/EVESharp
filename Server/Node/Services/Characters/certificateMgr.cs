using System.Collections.Generic;
using System.Reflection.Metadata.Ecma335;
using Common.Database;
using Common.Services;
using Node.Data;
using Node.Database;
using Node.Inventory;
using Node.Inventory.Items.Types;
using PythonTypes.Types.Complex;
using PythonTypes.Types.Exceptions;
using PythonTypes.Types.Primitives;
using SimpleInjector;

namespace Node.Services.Characters
{
    public class certificateMgr : Service
    {
        private CertificatesDB DB { get; }
        private ItemManager ItemManager { get; }
        private CacheStorage CacheStorage { get; }
        
        public certificateMgr(CertificatesDB db, ItemManager itemManager, CacheStorage cacheStorage)
        {
            this.DB = db;
            this.ItemManager = itemManager;
            this.CacheStorage = cacheStorage;
        }

        public PyDataType GetAllShipCertificateRecommendations(PyDictionary namedPayload, Client client)
        {
            this.CacheStorage.Load(
                "certificateMgr",
                "GetAllShipCertificateRecommendations",
                "SELECT shipTypeID, certificateID, recommendationLevel, recommendationID FROM crtRecommendations",
                CacheStorage.CacheObjectType.Rowset
            );

            PyDataType cacheHint = this.CacheStorage.GetHint("certificateMgr", "GetAllShipCertificateRecommendations");

            return PyCacheMethodCallResult.FromCacheHint(cacheHint);
        }

        public PyDataType GetCertificateCategories(PyDictionary namedPayload, Client client)
        {
            this.CacheStorage.Load(
                "certificateMgr",
                "GetCertificateCategories",
                "SELECT categoryID, categoryName, description, 0 AS dataID FROM crtCategories",
                CacheStorage.CacheObjectType.IndexRowset
            );

            PyDataType cacheHint = this.CacheStorage.GetHint("certificateMgr", "GetCertificateCategories");

            return PyCacheMethodCallResult.FromCacheHint(cacheHint);
        }

        public PyDataType GetCertificateClasses(PyDictionary namedPayload, Client client)
        {
            this.CacheStorage.Load(
                "certificateMgr",
                "GetCertificateClasses",
                "SELECT classID, className, description, 0 AS dataID FROM crtClasses",
                CacheStorage.CacheObjectType.IndexRowset
            );

            PyDataType cacheHint = this.CacheStorage.GetHint("certificateMgr", "GetCertificateClasses");

            return PyCacheMethodCallResult.FromCacheHint(cacheHint);
        }

        public PyDataType GetMyCertificates(PyDictionary namedPayload, Client client)
        {
            if (client.CharacterID == null)
                throw new UserError("NoCharacterSelected");

            return this.DB.GetMyCertificates((int) client.CharacterID);
        }

        public PyBool GrantCertificate(PyInteger certificateID, PyDictionary namedPayload, Client client)
        {
            if (client.CharacterID == null)
                throw new UserError("NoCharacterSelected");
            
            // TODO: WE MIGHT WANT TO CHECK AND ENSURE THAT THE CHARACTER BELONGS TO US BEFORE DOING ANYTHING ELSE HERE
            if (this.ItemManager.IsItemLoaded((int) client.CharacterID) == false)
                throw new CustomError("This request should arrive on the node that has this character loaded, not here");

            List<CertificateRelationship> requirements = this.DB.GetCertificateRequirements(certificateID);
            Character character = this.ItemManager.LoadItem((int) client.CharacterID) as Character;

            Dictionary<int, Skill> skills = character.InjectedSkillsByTypeID;

            foreach (CertificateRelationship relationship in requirements)
            {
                if (skills.ContainsKey(relationship.ParentTypeID) == false || skills[relationship.ParentTypeID].Level < relationship.ParentLevel)
                    return false;
            }
            
            // if this line is reached, the character has all the requirements for this cert
            this.DB.GrantCertificate((int) client.CharacterID, certificateID);
            
            return true;
        }

        public PyDataType BatchCertificateGrant(PyList certificateList, PyDictionary namedPayload, Client client)
        {
            if (client.CharacterID == null)
                throw new UserError("NoCharacterSelected");

            PyList result = new PyList();

            foreach (PyInteger certificateID in certificateList)
            {
                if (this.GrantCertificate(certificateID, namedPayload, client) == true)
                    result.Add(certificateID);
            }

            return result;
        }

        public PyDataType UpdateCertificateFlags(PyInteger certificateID, PyInteger visibilityFlags, PyDictionary namedPayload,
            Client client)
        {
            if (client.CharacterID == null)
                throw new UserError("NoCharacterSelected");

            this.DB.UpdateVisibilityFlags(certificateID, (int) client.CharacterID, visibilityFlags);
            
            return null;
        }

        public PyDataType BatchCertificateUpdate(PyDictionary updates, PyDictionary namedPayload, Client client)
        {
            if (client.CharacterID == null)
                throw new UserError("NoCharacterSelected");

            foreach (KeyValuePair<PyDataType, PyDataType> update in updates)
                this.UpdateCertificateFlags(update.Key as PyInteger, update.Value as PyInteger, namedPayload, client);

            return null;
        }

        public PyDataType GetCertificatesByCharacter(PyInteger characterID, PyDictionary namedPayload, Client client)
        {
            return this.DB.GetCertificatesByCharacter(characterID);
        }
    }
}
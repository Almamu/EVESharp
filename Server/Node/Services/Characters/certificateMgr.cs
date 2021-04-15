using System.Collections.Generic;
using Common.Services;
using Node.Database;
using Node.Exceptions.certificateMgr;
using Node.Inventory;
using Node.Inventory.Items.Types;
using Node.Network;
using Node.Notifications.Client.Certificates;
using Node.StaticData;
using Node.StaticData.Certificates;
using PythonTypes.Types.Collections;
using PythonTypes.Types.Complex;
using PythonTypes.Types.Exceptions;
using PythonTypes.Types.Primitives;

namespace Node.Services.Characters
{
    public class certificateMgr : IService
    {
        private CertificatesDB DB { get; }
        private ItemFactory ItemFactory { get; }
        private CacheStorage CacheStorage { get; }
        private Dictionary<int, List<Relationship>> CertificateRelationships { get; }
        
        public certificateMgr(CertificatesDB db, ItemFactory itemFactory, CacheStorage cacheStorage)
        {
            this.DB = db;
            this.ItemFactory = itemFactory;
            this.CacheStorage = cacheStorage;

            // get the full list of requirements
            this.CertificateRelationships = this.DB.GetCertificateRelationships();
        }

        public PyDataType GetAllShipCertificateRecommendations(CallInformation call)
        {
            this.CacheStorage.Load(
                "certificateMgr",
                "GetAllShipCertificateRecommendations",
                "SELECT shipTypeID, certificateID, recommendationLevel, recommendationID FROM crtRecommendations",
                CacheStorage.CacheObjectType.Rowset
            );

            return PyCacheMethodCallResult.FromCacheHint(
                this.CacheStorage.GetHint("certificateMgr", "GetAllShipCertificateRecommendations")
            );
        }

        public PyDataType GetCertificateCategories(CallInformation call)
        {
            this.CacheStorage.Load(
                "certificateMgr",
                "GetCertificateCategories",
                "SELECT categoryID, categoryName, description, 0 AS dataID FROM crtCategories",
                CacheStorage.CacheObjectType.IndexRowset
            );

            return PyCacheMethodCallResult.FromCacheHint(
                this.CacheStorage.GetHint("certificateMgr", "GetCertificateCategories")
            );
        }

        public PyDataType GetCertificateClasses(CallInformation call)
        {
            this.CacheStorage.Load(
                "certificateMgr",
                "GetCertificateClasses",
                "SELECT classID, className, description, 0 AS dataID FROM crtClasses",
                CacheStorage.CacheObjectType.IndexRowset
            );

            return PyCacheMethodCallResult.FromCacheHint(
                this.CacheStorage.GetHint("certificateMgr", "GetCertificateClasses")
            );
        }

        public PyDataType GetMyCertificates(CallInformation call)
        {
            return this.DB.GetMyCertificates(call.Client.EnsureCharacterIsSelected());
        }

        public PyBool GrantCertificate(PyInteger certificateID, CallInformation call)
        {
            int callerCharacterID = call.Client.EnsureCharacterIsSelected();
            Character character = this.ItemFactory.GetItem<Character>(callerCharacterID);

            Dictionary<int, Skill> skills = character.InjectedSkillsByTypeID;
            List<int> grantedCertificates = this.DB.GetCertificateListForCharacter(callerCharacterID);

            if (grantedCertificates.Contains(certificateID) == true)
                throw new CertificateAlreadyGranted();

            if (this.CertificateRelationships.TryGetValue(certificateID, out List<Relationship> requirements) == true)
            {
                foreach (Relationship relationship in requirements)
                {
                    if (relationship.ParentTypeID != 0 && (skills.TryGetValue(relationship.ParentTypeID, out Skill skill) == false || skill.Level < relationship.ParentLevel))
                        throw new CertificateSkillPrerequisitesNotMet();
                    if (relationship.ParentID != 0 && grantedCertificates.Contains(relationship.ParentID) == false)
                        throw new CertificateCertPrerequisitesNotMet();
                }
            }
            
            // if this line is reached, the character has all the requirements for this cert
            this.DB.GrantCertificate(callerCharacterID, certificateID);
            
            // notify the character about the granting of the certificate
            call.Client.NotifyMultiEvent(new OnCertificateIssued(certificateID));
            
            return null;
        }

        public PyDataType BatchCertificateGrant(PyList certificateList, CallInformation call)
        {
            int callerCharacterID = call.Client.EnsureCharacterIsSelected();
            Character character = this.ItemFactory.GetItem<Character>(callerCharacterID);

            PyList<PyInteger> result = new PyList<PyInteger>();
            Dictionary<int, Skill> skills = character.InjectedSkillsByTypeID;
            List<int> grantedCertificates = this.DB.GetCertificateListForCharacter(callerCharacterID);

            foreach (PyInteger certificateID in certificateList.GetEnumerable<PyInteger>())
            {
                if (this.CertificateRelationships.TryGetValue(certificateID, out List<Relationship> relationships) == true)
                {
                    bool requirementsMet = true;
                
                    foreach (Relationship relationship in relationships)
                    {
                        if (relationship.ParentTypeID != 0 && (skills.TryGetValue(relationship.ParentTypeID, out Skill skill) == false || skill.Level < relationship.ParentLevel))
                            requirementsMet = false;
                        if (relationship.ParentID != 0 && grantedCertificates.Contains(relationship.ParentID) == false)
                            requirementsMet = false;
                    }

                    if (requirementsMet == false)
                        continue;
                }
                
                // grant the certificate and add it to the list of granted certs
                this.DB.GrantCertificate(callerCharacterID, certificateID);
                // ensure the result includes that certificate list
                result.Add(certificateID);
                // add the cert to the list so certs that depend on others are properly granted
                grantedCertificates.Add(certificateID);
            }
            
            // notify the client about the granting of certificates
            call.Client.NotifyMultiEvent(new OnCertificateIssued());

            return result;
        }

        public PyDataType UpdateCertificateFlags(PyInteger certificateID, PyInteger visibilityFlags, CallInformation call)
        {
            this.DB.UpdateVisibilityFlags(certificateID, call.Client.EnsureCharacterIsSelected(), visibilityFlags);
            
            return null;
        }

        public PyDataType BatchCertificateUpdate(PyDictionary updates, CallInformation call)
        {
            call.Client.EnsureCharacterIsSelected();
            
            foreach ((PyInteger key, PyInteger value) in updates.GetEnumerable<PyInteger,PyInteger>())
                this.UpdateCertificateFlags(key, value, call);

            return null;
        }

        public PyDataType GetCertificatesByCharacter(PyInteger characterID, CallInformation call)
        {
            return this.DB.GetCertificatesByCharacter(characterID);
        }
    }
}
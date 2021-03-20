﻿using System.Collections.Generic;
using Common.Services;
using Node.Data;
using Node.Database;
using Node.Inventory;
using Node.Inventory.Items.Types;
using Node.Network;
using PythonTypes.Types.Collections;
using PythonTypes.Types.Complex;
using PythonTypes.Types.Exceptions;
using PythonTypes.Types.Primitives;

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

        public PyDataType GetAllShipCertificateRecommendations(CallInformation call)
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

        public PyDataType GetCertificateCategories(CallInformation call)
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

        public PyDataType GetCertificateClasses(CallInformation call)
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

        public PyDataType GetMyCertificates(CallInformation call)
        {
            return this.DB.GetMyCertificates(call.Client.EnsureCharacterIsSelected());
        }

        public PyBool GrantCertificate(PyInteger certificateID, CallInformation call)
        {
            int callerCharacterID = call.Client.EnsureCharacterIsSelected();
            
            if (this.ItemManager.TryGetItem(callerCharacterID, out Character character) == false)
                throw new CustomError("This request should arrive on the node that has this character loaded, not here");

            List<CertificateRelationship> requirements = this.DB.GetCertificateRequirements(certificateID);
            Dictionary<int, Skill> skills = character.InjectedSkillsByTypeID;

            foreach (CertificateRelationship relationship in requirements)
            {
                if (skills.ContainsKey(relationship.ParentTypeID) == false || skills[relationship.ParentTypeID].Level < relationship.ParentLevel)
                    return false;
            }
            
            // if this line is reached, the character has all the requirements for this cert
            this.DB.GrantCertificate(callerCharacterID, certificateID);
            
            return true;
        }

        public PyDataType BatchCertificateGrant(PyList certificateList, CallInformation call)
        {
            call.Client.EnsureCharacterIsSelected();

            PyList result = new PyList();

            foreach (PyInteger certificateID in certificateList)
            {
                if (this.GrantCertificate(certificateID, call) == true)
                    result.Add(certificateID);
            }

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
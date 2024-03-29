﻿using System.Collections.Generic;
using EVESharp.Database;
using EVESharp.Database.Certificates;
using EVESharp.Database.Extensions;
using EVESharp.EVE.Data.Inventory;
using EVESharp.EVE.Data.Inventory.Items.Types;
using EVESharp.EVE.Exceptions.certificateMgr;
using EVESharp.EVE.Network.Caching;
using EVESharp.EVE.Network.Services;
using EVESharp.EVE.Network.Services.Validators;
using EVESharp.EVE.Notifications;
using EVESharp.EVE.Notifications.Certificates;
using EVESharp.EVE.Packets.Complex;
using EVESharp.Types;
using EVESharp.Types.Collections;

namespace EVESharp.Node.Services.Characters;

[MustBeCharacter]
public class certificateMgr : Service
{
    public override AccessLevel                           AccessLevel              => AccessLevel.None;
    private         IItems                                Items                    { get; }
    private         ICacheStorage                         CacheStorage             { get; }
    private         Dictionary <int, List <Relationship>> CertificateRelationships { get; }
    private         IDogmaNotifications                   DogmaNotifications       { get; }
    private         IDatabase                   Database                 { get; }

    public certificateMgr (IItems items, ICacheStorage cacheStorage, IDogmaNotifications dogmaNotifications, IDatabase database)
    {
        Database                = database;
        this.Items              = items;
        CacheStorage            = cacheStorage;
        this.DogmaNotifications = dogmaNotifications;

        // get the full list of requirements
        CertificateRelationships = Database.GetCertificateRelationships ();
    }

    public PyDataType GetAllShipCertificateRecommendations (ServiceCall call)
    {
        CacheStorage.Load (
            "certificateMgr",
            "GetAllShipCertificateRecommendations",
            "SELECT shipTypeID, certificateID, recommendationLevel, recommendationID FROM crtRecommendations",
            CacheObjectType.Rowset
        );

        return CachedMethodCallResult.FromCacheHint (CacheStorage.GetHint ("certificateMgr", "GetAllShipCertificateRecommendations"));
    }

    public PyDataType GetCertificateCategories (ServiceCall call)
    {
        CacheStorage.Load (
            "certificateMgr",
            "GetCertificateCategories",
            "SELECT categoryID, categoryName, description, 0 AS dataID FROM crtCategories",
            CacheObjectType.IndexRowset
        );

        return CachedMethodCallResult.FromCacheHint (CacheStorage.GetHint ("certificateMgr", "GetCertificateCategories"));
    }

    public PyDataType GetCertificateClasses (ServiceCall call)
    {
        CacheStorage.Load (
            "certificateMgr",
            "GetCertificateClasses",
            "SELECT classID, className, description, 0 AS dataID FROM crtClasses",
            CacheObjectType.IndexRowset
        );

        return CachedMethodCallResult.FromCacheHint (CacheStorage.GetHint ("certificateMgr", "GetCertificateClasses"));
    }

    public PyDataType GetMyCertificates (ServiceCall call)
    {
        return Database.CrtGetCharacterCertificates (call.Session.CharacterID);
    }

    public PyBool GrantCertificate (ServiceCall call, PyInteger certificateID)
    {
        int       callerCharacterID = call.Session.CharacterID;
        Character character         = this.Items.GetItem <Character> (callerCharacterID);

        Dictionary <int, Skill> skills              = character.InjectedSkillsByTypeID;
        List <int>              grantedCertificates = Database.GetCertificateListForCharacter (callerCharacterID);

        if (grantedCertificates.Contains (certificateID))
            throw new CertificateAlreadyGranted ();

        if (CertificateRelationships.TryGetValue (certificateID, out List <Relationship> requirements))
            foreach (Relationship relationship in requirements)
            {
                if (relationship.ParentTypeID != 0 &&
                    (skills.TryGetValue (relationship.ParentTypeID, out Skill skill) == false || skill.Level < relationship.ParentLevel))
                    throw new CertificateSkillPrerequisitesNotMet ();

                if (relationship.ParentID != 0 && grantedCertificates.Contains (relationship.ParentID) == false)
                    throw new CertificateCertPrerequisitesNotMet ();
            }

        // if this line is reached, the character has all the requirements for this cert
        Database.CrtGrantCertificate (callerCharacterID, certificateID);

        // notify the character about the granting of the certificate
        this.DogmaNotifications.QueueMultiEvent (callerCharacterID, new OnCertificateIssued (certificateID));

        return null;
    }

    public PyDataType BatchCertificateGrant (ServiceCall call, PyList certificateList)
    {
        int       callerCharacterID = call.Session.CharacterID;
        Character character         = this.Items.GetItem <Character> (callerCharacterID);

        PyList <PyInteger>      result              = new PyList <PyInteger> ();
        Dictionary <int, Skill> skills              = character.InjectedSkillsByTypeID;
        List <int>              grantedCertificates = Database.GetCertificateListForCharacter (callerCharacterID);

        foreach (PyInteger certificateID in certificateList.GetEnumerable <PyInteger> ())
        {
            if (CertificateRelationships.TryGetValue (certificateID, out List <Relationship> relationships))
            {
                bool requirementsMet = true;

                foreach (Relationship relationship in relationships)
                {
                    if (relationship.ParentTypeID != 0 &&
                        (skills.TryGetValue (relationship.ParentTypeID, out Skill skill) == false || skill.Level < relationship.ParentLevel))
                        requirementsMet = false;

                    if (relationship.ParentID != 0 && grantedCertificates.Contains (relationship.ParentID) == false)
                        requirementsMet = false;
                }

                if (requirementsMet == false)
                    continue;
            }

            // grant the certificate and add it to the list of granted certs
            Database.CrtGrantCertificate (callerCharacterID, certificateID);
            // ensure the result includes that certificate list
            result.Add (certificateID);
            // add the cert to the list so certs that depend on others are properly granted
            grantedCertificates.Add (certificateID);
        }

        // notify the client about the granting of certificates
        this.DogmaNotifications.QueueMultiEvent (callerCharacterID, new OnCertificateIssued ());

        return result;
    }

    public PyDataType UpdateCertificateFlags (ServiceCall call, PyInteger certificateID, PyInteger visibilityFlags)
    {
        Database.CrtUpdateVisibilityFlags (call.Session.CharacterID, certificateID, visibilityFlags);

        return null;
    }

    public PyDataType BatchCertificateUpdate (ServiceCall call, PyDictionary updates)
    {
        foreach ((PyInteger key, PyInteger value) in updates.GetEnumerable <PyInteger, PyInteger> ())
            this.UpdateCertificateFlags (call, key, value);

        return null;
    }

    public PyDataType GetCertificatesByCharacter (ServiceCall call, PyInteger characterID)
    {
        return Database.CrtGetCharacterCertificates (characterID);
    }
}
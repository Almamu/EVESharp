using System.Collections.Generic;
using EVESharp.Database;
using EVESharp.EVE.Client.Exceptions.certificateMgr;
using EVESharp.EVE.Packets.Complex;
using EVESharp.EVE.Services;
using EVESharp.EVE.Services.Validators;
using EVESharp.EVE.StaticData.Certificates;
using EVESharp.Node.Cache;
using EVESharp.Node.Client.Notifications.Certificates;
using EVESharp.Node.Dogma;
using EVESharp.Node.Inventory;
using EVESharp.Node.Inventory.Items.Types;
using EVESharp.PythonTypes.Types.Collections;
using EVESharp.PythonTypes.Types.Database;
using EVESharp.PythonTypes.Types.Primitives;

namespace EVESharp.Node.Services.Characters;

[MustBeCharacter]
public class certificateMgr : Service
{
    public override AccessLevel                           AccessLevel              => AccessLevel.None;
    private         ItemFactory                           ItemFactory              { get; }
    private         CacheStorage                          CacheStorage             { get; }
    private         Dictionary <int, List <Relationship>> CertificateRelationships { get; }
    private         DogmaUtils                            DogmaUtils               { get; }
    private         IDatabaseConnection                   Database                 { get; }

    public certificateMgr (ItemFactory itemFactory, CacheStorage cacheStorage, DogmaUtils dogmaUtils, IDatabaseConnection database)
    {
        Database     = database;
        ItemFactory  = itemFactory;
        CacheStorage = cacheStorage;
        DogmaUtils   = dogmaUtils;

        // get the full list of requirements
        CertificateRelationships = Database.GetCertificateRelationships ();
    }

    public PyDataType GetAllShipCertificateRecommendations (CallInformation call)
    {
        CacheStorage.Load (
            "certificateMgr",
            "GetAllShipCertificateRecommendations",
            "SELECT shipTypeID, certificateID, recommendationLevel, recommendationID FROM crtRecommendations",
            CacheStorage.CacheObjectType.Rowset
        );

        return CachedMethodCallResult.FromCacheHint (CacheStorage.GetHint ("certificateMgr", "GetAllShipCertificateRecommendations"));
    }

    public PyDataType GetCertificateCategories (CallInformation call)
    {
        CacheStorage.Load (
            "certificateMgr",
            "GetCertificateCategories",
            "SELECT categoryID, categoryName, description, 0 AS dataID FROM crtCategories",
            CacheStorage.CacheObjectType.IndexRowset
        );

        return CachedMethodCallResult.FromCacheHint (CacheStorage.GetHint ("certificateMgr", "GetCertificateCategories"));
    }

    public PyDataType GetCertificateClasses (CallInformation call)
    {
        CacheStorage.Load (
            "certificateMgr",
            "GetCertificateClasses",
            "SELECT classID, className, description, 0 AS dataID FROM crtClasses",
            CacheStorage.CacheObjectType.IndexRowset
        );

        return CachedMethodCallResult.FromCacheHint (CacheStorage.GetHint ("certificateMgr", "GetCertificateClasses"));
    }

    public PyDataType GetMyCertificates (CallInformation call)
    {
        return Database.CrtGetCharacterCertificates (call.Session.CharacterID);
    }

    public PyBool GrantCertificate (CallInformation call, PyInteger certificateID)
    {
        int       callerCharacterID = call.Session.CharacterID;
        Character character         = ItemFactory.GetItem <Character> (callerCharacterID);

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
        DogmaUtils.QueueMultiEvent (callerCharacterID, new OnCertificateIssued (certificateID));

        return null;
    }

    public PyDataType BatchCertificateGrant (CallInformation call, PyList certificateList)
    {
        int       callerCharacterID = call.Session.CharacterID;
        Character character         = ItemFactory.GetItem <Character> (callerCharacterID);

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
        DogmaUtils.QueueMultiEvent (callerCharacterID, new OnCertificateIssued ());

        return result;
    }

    public PyDataType UpdateCertificateFlags (CallInformation call, PyInteger certificateID, PyInteger visibilityFlags)
    {
        Database.CrtUpdateVisibilityFlags (call.Session.CharacterID, certificateID, visibilityFlags);

        return null;
    }

    public PyDataType BatchCertificateUpdate (CallInformation call, PyDictionary updates)
    {
        foreach ((PyInteger key, PyInteger value) in updates.GetEnumerable <PyInteger, PyInteger> ())
            this.UpdateCertificateFlags (call, key, value);

        return null;
    }

    public PyDataType GetCertificatesByCharacter (CallInformation call, PyInteger characterID)
    {
        return Database.CrtGetCharacterCertificates (characterID);
    }
}
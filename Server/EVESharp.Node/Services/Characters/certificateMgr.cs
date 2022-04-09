using System.Collections.Generic;
using EVESharp.EVE.Client.Exceptions.certificateMgr;
using EVESharp.EVE.Packets.Complex;
using EVESharp.EVE.Services;
using EVESharp.EVE.StaticData.Certificates;
using EVESharp.Node.Cache;
using EVESharp.Node.Database;
using EVESharp.Node.Inventory;
using EVESharp.Node.Inventory.Items.Types;
using EVESharp.Node.Network;
using EVESharp.Node.Notifications.Client.Certificates;
using EVESharp.Node.Sessions;
using EVESharp.PythonTypes.Types.Collections;
using EVESharp.PythonTypes.Types.Primitives;

namespace EVESharp.Node.Services.Characters;

public class certificateMgr : Service
{
    public override AccessLevel                           AccessLevel              => AccessLevel.None;
    private         CertificatesDB                        DB                       { get; }
    private         ItemFactory                           ItemFactory              { get; }
    private         CacheStorage                          CacheStorage             { get; }
    private         Dictionary <int, List <Relationship>> CertificateRelationships { get; }
    private         Node.Dogma.Dogma                      Dogma                    { get; }

    public certificateMgr (CertificatesDB db, ItemFactory itemFactory, CacheStorage cacheStorage, Node.Dogma.Dogma dogma)
    {
        DB           = db;
        ItemFactory  = itemFactory;
        CacheStorage = cacheStorage;
        Dogma        = dogma;

        // get the full list of requirements
        CertificateRelationships = DB.GetCertificateRelationships ();
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
        return DB.GetMyCertificates (call.Session.EnsureCharacterIsSelected ());
    }

    public PyBool GrantCertificate (PyInteger certificateID, CallInformation call)
    {
        int       callerCharacterID = call.Session.EnsureCharacterIsSelected ();
        Character character         = ItemFactory.GetItem <Character> (callerCharacterID);

        Dictionary <int, Skill> skills              = character.InjectedSkillsByTypeID;
        List <int>              grantedCertificates = DB.GetCertificateListForCharacter (callerCharacterID);

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
        DB.GrantCertificate (callerCharacterID, certificateID);

        // notify the character about the granting of the certificate
        Dogma.QueueMultiEvent (callerCharacterID, new OnCertificateIssued (certificateID));

        return null;
    }

    public PyDataType BatchCertificateGrant (PyList certificateList, CallInformation call)
    {
        int       callerCharacterID = call.Session.EnsureCharacterIsSelected ();
        Character character         = ItemFactory.GetItem <Character> (callerCharacterID);

        PyList <PyInteger>      result              = new PyList <PyInteger> ();
        Dictionary <int, Skill> skills              = character.InjectedSkillsByTypeID;
        List <int>              grantedCertificates = DB.GetCertificateListForCharacter (callerCharacterID);

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
            DB.GrantCertificate (callerCharacterID, certificateID);
            // ensure the result includes that certificate list
            result.Add (certificateID);
            // add the cert to the list so certs that depend on others are properly granted
            grantedCertificates.Add (certificateID);
        }

        // notify the client about the granting of certificates
        Dogma.QueueMultiEvent (callerCharacterID, new OnCertificateIssued ());

        return result;
    }

    public PyDataType UpdateCertificateFlags (PyInteger certificateID, PyInteger visibilityFlags, CallInformation call)
    {
        DB.UpdateVisibilityFlags (certificateID, call.Session.EnsureCharacterIsSelected (), visibilityFlags);

        return null;
    }

    public PyDataType BatchCertificateUpdate (PyDictionary updates, CallInformation call)
    {
        call.Session.EnsureCharacterIsSelected ();

        foreach ((PyInteger key, PyInteger value) in updates.GetEnumerable <PyInteger, PyInteger> ())
            this.UpdateCertificateFlags (key, value, call);

        return null;
    }

    public PyDataType GetCertificatesByCharacter (PyInteger characterID, CallInformation call)
    {
        return DB.GetCertificatesByCharacter (characterID);
    }
}
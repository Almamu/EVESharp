using EVESharp.Database.Old;
using EVESharp.EVE.Network.Services;
using EVESharp.Types;

namespace EVESharp.Node.Services.Data;

public class lookupSvc : Service
{
    public override AccessLevel AccessLevel => AccessLevel.None;
    private         LookupDB    DB          { get; }

    public lookupSvc (LookupDB db)
    {
        DB = db;
    }

    public PyDataType LookupPlayerCharacters (ServiceCall call, PyString criteria, PyDataType exactMatch)
    {
        bool exact = false;

        if (exactMatch is PyBool exactBool)
            exact = exactBool;
        else if (exactMatch is PyInteger exactInteger)
            exact = exactInteger != 0;

        return DB.LookupPlayerCharacters (criteria, exact);
    }

    public PyDataType LookupOwners (ServiceCall call, PyString criteria, PyDataType exactMatch)
    {
        bool exact = false;

        if (exactMatch is PyBool exactBool)
            exact = exactBool;
        else if (exactMatch is PyInteger exactInteger)
            exact = exactInteger != 0;

        return DB.LookupOwners (criteria, exact);
    }

    public PyDataType LookupCharacters (ServiceCall call, PyString criteria, PyDataType exactMatch)
    {
        bool exact = false;

        if (exactMatch is PyBool exactBool)
            exact = exactBool;
        else if (exactMatch is PyInteger exactInteger)
            exact = exactInteger != 0;

        return DB.LookupCharacters (criteria, exact);
    }

    public PyDataType LookupStations (ServiceCall call, PyString criteria, PyDataType exactMatch)
    {
        bool exact = false;

        if (exactMatch is PyBool exactBool)
            exact = exactBool;
        else if (exactMatch is PyInteger exactInteger)
            exact = exactInteger != 0;

        return DB.LookupStations (criteria, exact);
    }

    public PyDataType LookupCorporations (ServiceCall call, PyString criteria, PyDataType exactMatch)
    {
        bool exact = false;

        if (exactMatch is PyBool exactBool)
            exact = exactBool;
        else if (exactMatch is PyInteger exactInteger)
            exact = exactInteger != 0;

        return DB.LookupCorporations (criteria, exact);
    }

    public PyDataType LookupAlliances (ServiceCall call, PyString criteria, PyDataType exactMatch)
    {
        bool exact = false;

        if (exactMatch is PyBool exactBool)
            exact = exactBool;
        else if (exactMatch is PyInteger exactInteger)
            exact = exactInteger != 0;

        return DB.LookupAlliances (criteria, exact);
    }

    public PyDataType LookupAllianceShortNames (ServiceCall call, PyString criteria, PyDataType exactMatch)
    {
        bool exact = false;

        if (exactMatch is PyBool exactBool)
            exact = exactBool;
        else if (exactMatch is PyInteger exactInteger)
            exact = exactInteger != 0;

        return DB.LookupAllianceShortNames (criteria, exact);
    }

    public PyDataType LookupCorporationsOrAlliances (ServiceCall call, PyString criteria, PyDataType exactMatch)
    {
        bool exact = false;

        if (exactMatch is PyBool exactBool)
            exact = exactBool;
        else if (exactMatch is PyInteger exactInteger)
            exact = exactInteger != 0;

        return DB.LookupCorporationsOrAlliances (criteria, exact);
    }

    public PyDataType LookupCorporationsOrAlliances (ServiceCall call, PyString criteria, PyDataType exactMatch, PyDataType warableEntitysOnly)
    {
        bool exact = false;

        if (exactMatch is PyBool exactBool)
            exact = exactBool;
        else if (exactMatch is PyInteger exactInteger)
            exact = exactInteger != 0;

        bool warable = false;

        if (warableEntitysOnly is PyBool warableBool)
            warable = warableBool;
        else if (warableEntitysOnly is PyInteger warableInteger)
            warable = warableInteger != 0;

        if (warable)
            return DB.LookupWarableCorporationsOrAlliances (criteria, exact);

        return DB.LookupCorporationsOrAlliances (criteria, exact);
    }

    public PyDataType LookupCorporationTickers (ServiceCall call, PyString criteria, PyDataType exactMatch)
    {
        bool exact = false;

        if (exactMatch is PyBool exactBool)
            exact = exactBool;
        else if (exactMatch is PyInteger exactInteger)
            exact = exactInteger != 0;

        return DB.LookupCorporationTickers (criteria, exact);
    }

    public PyDataType LookupFactions (ServiceCall call, PyString criteria, PyDataType exactMatch)
    {
        bool exact = false;

        if (exactMatch is PyBool exactBool)
            exact = exactBool;
        else if (exactMatch is PyInteger exactInteger)
            exact = exactInteger != 0;

        return DB.LookupFactions (criteria, exact);
    }

    public PyDataType LookupKnownLocationsByGroup (ServiceCall call, PyInteger groupID, PyString searchStr)
    {
        return DB.LookupKnownLocationsByGroup (searchStr, groupID);
    }
}
using EVESharp.EVE.Services;
using EVESharp.Node.Database;
using EVESharp.PythonTypes.Types.Primitives;

namespace EVESharp.Node.Services.Data;

public class lookupSvc : Service
{
    public override AccessLevel AccessLevel => AccessLevel.None;
    private         LookupDB    DB          { get; }

    public lookupSvc (LookupDB db)
    {
        DB = db;
    }

    public PyDataType LookupPlayerCharacters (CallInformation call, PyString criteria, PyDataType exactMatch)
    {
        bool exact = false;

        if (exactMatch is PyBool exactBool)
            exact = exactBool;
        else if (exactMatch is PyInteger exactInteger)
            exact = exactInteger != 0;

        return DB.LookupPlayerCharacters (criteria, exact);
    }

    public PyDataType LookupOwners (CallInformation call, PyString criteria, PyDataType exactMatch)
    {
        bool exact = false;

        if (exactMatch is PyBool exactBool)
            exact = exactBool;
        else if (exactMatch is PyInteger exactInteger)
            exact = exactInteger != 0;

        return DB.LookupOwners (criteria, exact);
    }

    public PyDataType LookupCharacters (CallInformation call, PyString criteria, PyDataType exactMatch)
    {
        bool exact = false;

        if (exactMatch is PyBool exactBool)
            exact = exactBool;
        else if (exactMatch is PyInteger exactInteger)
            exact = exactInteger != 0;

        return DB.LookupCharacters (criteria, exact);
    }

    public PyDataType LookupStations (CallInformation call, PyString criteria, PyDataType exactMatch)
    {
        bool exact = false;

        if (exactMatch is PyBool exactBool)
            exact = exactBool;
        else if (exactMatch is PyInteger exactInteger)
            exact = exactInteger != 0;

        return DB.LookupStations (criteria, exact);
    }

    public PyDataType LookupCorporations (CallInformation call, PyString criteria, PyDataType exactMatch)
    {
        bool exact = false;

        if (exactMatch is PyBool exactBool)
            exact = exactBool;
        else if (exactMatch is PyInteger exactInteger)
            exact = exactInteger != 0;

        return DB.LookupCorporations (criteria, exact);
    }

    public PyDataType LookupAlliances (CallInformation call, PyString criteria, PyDataType exactMatch)
    {
        bool exact = false;

        if (exactMatch is PyBool exactBool)
            exact = exactBool;
        else if (exactMatch is PyInteger exactInteger)
            exact = exactInteger != 0;

        return DB.LookupAlliances (criteria, exact);
    }

    public PyDataType LookupAllianceShortNames (CallInformation call, PyString criteria, PyDataType exactMatch)
    {
        bool exact = false;

        if (exactMatch is PyBool exactBool)
            exact = exactBool;
        else if (exactMatch is PyInteger exactInteger)
            exact = exactInteger != 0;

        return DB.LookupAllianceShortNames (criteria, exact);
    }

    public PyDataType LookupCorporationsOrAlliances (CallInformation call, PyString criteria, PyDataType exactMatch)
    {
        bool exact = false;

        if (exactMatch is PyBool exactBool)
            exact = exactBool;
        else if (exactMatch is PyInteger exactInteger)
            exact = exactInteger != 0;

        return DB.LookupCorporationsOrAlliances (criteria, exact);
    }

    public PyDataType LookupCorporationsOrAlliances (CallInformation call, PyString criteria, PyDataType exactMatch, PyDataType warableEntitysOnly)
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

    public PyDataType LookupCorporationTickers (CallInformation call, PyString criteria, PyDataType exactMatch)
    {
        bool exact = false;

        if (exactMatch is PyBool exactBool)
            exact = exactBool;
        else if (exactMatch is PyInteger exactInteger)
            exact = exactInteger != 0;

        return DB.LookupCorporationTickers (criteria, exact);
    }

    public PyDataType LookupFactions (CallInformation call, PyString criteria, PyDataType exactMatch)
    {
        bool exact = false;

        if (exactMatch is PyBool exactBool)
            exact = exactBool;
        else if (exactMatch is PyInteger exactInteger)
            exact = exactInteger != 0;

        return DB.LookupFactions (criteria, exact);
    }

    public PyDataType LookupKnownLocationsByGroup (CallInformation call, PyInteger groupID, PyString searchStr)
    {
        return DB.LookupKnownLocationsByGroup (searchStr, groupID);
    }
}
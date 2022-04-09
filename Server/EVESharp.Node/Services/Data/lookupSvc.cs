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

    public PyDataType LookupPlayerCharacters (PyString criteria, PyDataType exactMatch, CallInformation call)
    {
        bool exact = false;

        if (exactMatch is PyBool exactBool)
            exact = exactBool;
        else if (exactMatch is PyInteger exactInteger)
            exact = exactInteger != 0;

        return DB.LookupPlayerCharacters (criteria, exact);
    }

    public PyDataType LookupOwners (PyString criteria, PyDataType exactMatch, CallInformation call)
    {
        bool exact = false;

        if (exactMatch is PyBool exactBool)
            exact = exactBool;
        else if (exactMatch is PyInteger exactInteger)
            exact = exactInteger != 0;

        return DB.LookupOwners (criteria, exact);
    }

    public PyDataType LookupCharacters (PyString criteria, PyDataType exactMatch, CallInformation call)
    {
        bool exact = false;

        if (exactMatch is PyBool exactBool)
            exact = exactBool;
        else if (exactMatch is PyInteger exactInteger)
            exact = exactInteger != 0;

        return DB.LookupCharacters (criteria, exact);
    }

    public PyDataType LookupStations (PyString criteria, PyDataType exactMatch, CallInformation call)
    {
        bool exact = false;

        if (exactMatch is PyBool exactBool)
            exact = exactBool;
        else if (exactMatch is PyInteger exactInteger)
            exact = exactInteger != 0;

        return DB.LookupStations (criteria, exact);
    }

    public PyDataType LookupCorporations (PyString criteria, PyDataType exactMatch, CallInformation call)
    {
        bool exact = false;

        if (exactMatch is PyBool exactBool)
            exact = exactBool;
        else if (exactMatch is PyInteger exactInteger)
            exact = exactInteger != 0;

        return DB.LookupCorporations (criteria, exact);
    }

    public PyDataType LookupAlliances (PyString criteria, PyDataType exactMatch, CallInformation call)
    {
        bool exact = false;

        if (exactMatch is PyBool exactBool)
            exact = exactBool;
        else if (exactMatch is PyInteger exactInteger)
            exact = exactInteger != 0;

        return DB.LookupAlliances (criteria, exact);
    }

    public PyDataType LookupAllianceShortNames (PyString criteria, PyDataType exactMatch, CallInformation call)
    {
        bool exact = false;

        if (exactMatch is PyBool exactBool)
            exact = exactBool;
        else if (exactMatch is PyInteger exactInteger)
            exact = exactInteger != 0;

        return DB.LookupAllianceShortNames (criteria, exact);
    }

    public PyDataType LookupCorporationsOrAlliances (PyString criteria, PyDataType exactMatch, CallInformation call)
    {
        bool exact = false;

        if (exactMatch is PyBool exactBool)
            exact = exactBool;
        else if (exactMatch is PyInteger exactInteger)
            exact = exactInteger != 0;

        return DB.LookupCorporationsOrAlliances (criteria, exact);
    }

    public PyDataType LookupCorporationsOrAlliances (PyString criteria, PyDataType exactMatch, PyDataType warableEntitysOnly, CallInformation call)
    {
        // TODO: warableEntitysOnly
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

    public PyDataType LookupCorporationTickers (PyString criteria, PyDataType exactMatch, CallInformation call)
    {
        bool exact = false;

        if (exactMatch is PyBool exactBool)
            exact = exactBool;
        else if (exactMatch is PyInteger exactInteger)
            exact = exactInteger != 0;

        return DB.LookupCorporationTickers (criteria, exact);
    }

    public PyDataType LookupFactions (PyString criteria, PyDataType exactMatch, CallInformation call)
    {
        bool exact = false;

        if (exactMatch is PyBool exactBool)
            exact = exactBool;
        else if (exactMatch is PyInteger exactInteger)
            exact = exactInteger != 0;

        return DB.LookupFactions (criteria, exact);
    }

    public PyDataType LookupKnownLocationsByGroup (PyInteger groupID, PyString searchStr, CallInformation call)
    {
        return DB.LookupKnownLocationsByGroup (searchStr, groupID);
    }
}
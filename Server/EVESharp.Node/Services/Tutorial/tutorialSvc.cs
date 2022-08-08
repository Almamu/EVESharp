using EVESharp.EVE.Packets.Complex;
using EVESharp.EVE.Services;
using EVESharp.Node.Cache;
using EVESharp.Node.Database;
using EVESharp.PythonTypes.Types.Collections;
using EVESharp.PythonTypes.Types.Database;
using EVESharp.PythonTypes.Types.Primitives;

namespace EVESharp.Node.Services.Tutorial;

public class tutorialSvc : Service
{
    public override AccessLevel  AccessLevel  => AccessLevel.Station;
    private         CacheStorage CacheStorage { get; }
    private         TutorialsDB  DB           { get; }

    public tutorialSvc (CacheStorage cacheStorage, TutorialsDB db)
    {
        CacheStorage = cacheStorage;
        DB           = db;
    }

    public PyDataType GetCharacterTutorialState (CallInformation call)
    {
        return new Rowset (
            new PyList <PyString> (4)
            {
                [0] = "characterID",
                [1] = "tutorialID",
                [2] = "pageID",
                [3] = "eventTypeID"
            }
        );
    }

    public PyList GetContextHelp (CallInformation call)
    {
        return new PyList ();
    }

    public PyDataType GetTutorials (CallInformation call)
    {
        CacheStorage.Load (
            "tutorialSvc",
            "GetTutorials",
            "SELECT tutorialID, tutorialName, nextTutorialID, categoryID, 0 AS dataID FROM tutorials RIGHT JOIN tutorial_pages USING(tutorialID)",
            CacheObjectType.CRowset
        );

        PyDataType cacheHint = CacheStorage.GetHint ("tutorialSvc", "GetTutorials");

        return CachedMethodCallResult.FromCacheHint (cacheHint);
    }

    public PyDataType GetCategories (CallInformation call)
    {
        CacheStorage.Load (
            "tutorialSvc",
            "GetCategories",
            "SELECT categoryID, categoryName, description, 0 AS dataID FROM tutorial_categories",
            CacheObjectType.CRowset
        );

        PyDataType cacheHint = CacheStorage.GetHint ("tutorialSvc", "GetCategories");

        return CachedMethodCallResult.FromCacheHint (cacheHint);
    }

    public PyDataType GetTutorialInfo (CallInformation call, PyInteger tutorialID)
    {
        return DB.GetTutorialInfo (tutorialID);
    }

    public PyDataType GetCriterias (CallInformation call)
    {
        CacheStorage.Load (
            "tutorialSvc",
            "GetCriterias",
            "SELECT criteriaID, criteriaName, messageText, audioPath, 0 AS dataID FROM tutorial_criteria",
            CacheObjectType.CRowset
        );

        PyDataType cacheHint = CacheStorage.GetHint ("tutorialSvc", "GetCriterias");

        return CachedMethodCallResult.FromCacheHint (cacheHint);
    }

    public PyDataType GetTutorialGoodies (CallInformation call, PyInteger tutorialID, PyInteger pageID, PyInteger pageNumber)
    {
        // there's not tutorial goodies that we know of
        return new PyList ();
    }

    public PyDataType LogStarted (
        CallInformation call, PyInteger       tutorialID, PyInteger pageNumber, PyInteger secondsAfterOpeningTutorial
    )
    {
        // there's no need to log when the tutorial started
        // no interest in this kind of metrics
        return null;
    }

    public PyDataType LogCompleted (
        CallInformation call, PyInteger tutorialID,                  PyInteger       pageNumber,
        PyInteger secondsAfterOpeningTutorial
    )
    {
        // there's no need to log when the tutorial was completed
        // no interest in this kind of metrics
        return null;
    }

    public PyDataType LogAborted (
        CallInformation call, PyInteger tutorialID,                  PyInteger       pageNumber,
        PyInteger secondsAfterOpeningTutorial
    )
    {
        // there's no need to log when the tutorial was aborted
        // no interest in this kind of metrics
        return null;
    }

    public PyDataType GetTutorialAgents (CallInformation call, PyList agentIDs)
    {
        return DB.GetTutorialAgents (agentIDs.GetEnumerable <PyInteger> ());
    }
}
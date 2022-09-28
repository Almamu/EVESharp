using EVESharp.Database.Old;
using EVESharp.EVE.Network.Caching;
using EVESharp.EVE.Network.Services;
using EVESharp.EVE.Packets.Complex;
using EVESharp.EVE.Types;
using EVESharp.Types;
using EVESharp.Types.Collections;

namespace EVESharp.Node.Services.Tutorial;

public class tutorialSvc : Service
{
    public override AccessLevel   AccessLevel  => AccessLevel.Station;
    private         ICacheStorage CacheStorage { get; }
    private         TutorialsDB   DB           { get; }

    public tutorialSvc (ICacheStorage cacheStorage, TutorialsDB db)
    {
        CacheStorage = cacheStorage;
        DB           = db;
    }

    public PyDataType GetCharacterTutorialState (ServiceCall call)
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

    public PyList GetContextHelp (ServiceCall call)
    {
        return new PyList ();
    }

    public PyDataType GetTutorials (ServiceCall call)
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

    public PyDataType GetCategories (ServiceCall call)
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

    public PyDataType GetTutorialInfo (ServiceCall call, PyInteger tutorialID)
    {
        return DB.GetTutorialInfo (tutorialID);
    }

    public PyDataType GetCriterias (ServiceCall call)
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

    public PyDataType GetTutorialGoodies (ServiceCall call, PyInteger tutorialID, PyInteger pageID, PyInteger pageNumber)
    {
        // there's not tutorial goodies that we know of
        return new PyList ();
    }

    public PyDataType LogStarted
    (
        ServiceCall call, PyInteger tutorialID, PyInteger pageNumber, PyInteger secondsAfterOpeningTutorial
    )
    {
        // there's no need to log when the tutorial started
        // no interest in this kind of metrics
        return null;
    }

    public PyDataType LogCompleted
    (
        ServiceCall call, PyInteger tutorialID, PyInteger pageNumber,
        PyInteger       secondsAfterOpeningTutorial
    )
    {
        // there's no need to log when the tutorial was completed
        // no interest in this kind of metrics
        return null;
    }

    public PyDataType LogAborted
    (
        ServiceCall call, PyInteger tutorialID, PyInteger pageNumber,
        PyInteger       secondsAfterOpeningTutorial
    )
    {
        // there's no need to log when the tutorial was aborted
        // no interest in this kind of metrics
        return null;
    }

    public PyDataType GetTutorialAgents (ServiceCall call, PyList agentIDs)
    {
        return DB.GetTutorialAgents (agentIDs.GetEnumerable <PyInteger> ());
    }
}
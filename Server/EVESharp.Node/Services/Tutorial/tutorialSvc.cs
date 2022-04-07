using EVESharp.EVE.Packets.Complex;
using EVESharp.EVE.Services;
using EVESharp.Node.Database;
using EVESharp.Node.Network;
using EVESharp.PythonTypes.Types.Collections;
using EVESharp.PythonTypes.Types.Database;
using EVESharp.PythonTypes.Types.Primitives;

namespace EVESharp.Node.Services.Tutorial;

public class tutorialSvc : Service
{
    public override AccessLevel  AccessLevel  => AccessLevel.Station;
    private         CacheStorage CacheStorage { get; init; }
    private         TutorialsDB  DB           { get; init; }
        
    public tutorialSvc(CacheStorage cacheStorage, TutorialsDB db)
    {
        this.CacheStorage = cacheStorage;
        this.DB           = db;
    }

    public PyDataType GetCharacterTutorialState(CallInformation call)
    {
        return new Rowset(new PyList<PyString>(4)
        {
            [0] = "characterID",
            [1] = "tutorialID",
            [2] = "pageID",
            [3] = "eventTypeID"
        });
    }

    public PyList GetContextHelp(CallInformation call)
    {
        return new PyList();
    }

    public PyDataType GetTutorials(CallInformation call)
    {
        this.CacheStorage.Load(
            "tutorialSvc",
            "GetTutorials",
            "SELECT tutorialID, tutorialName, nextTutorialID, categoryID, 0 AS dataID FROM tutorials RIGHT JOIN tutorial_pages USING(tutorialID)",
            CacheStorage.CacheObjectType.CRowset
        );

        PyDataType cacheHint = this.CacheStorage.GetHint("tutorialSvc", "GetTutorials");

        return CachedMethodCallResult.FromCacheHint(cacheHint);
    }

    public PyDataType GetCategories(CallInformation call)
    {
        this.CacheStorage.Load(
            "tutorialSvc",
            "GetCategories",
            "SELECT categoryID, categoryName, description, 0 AS dataID FROM tutorial_categories",
            CacheStorage.CacheObjectType.CRowset
        );

        PyDataType cacheHint = this.CacheStorage.GetHint("tutorialSvc", "GetCategories");

        return CachedMethodCallResult.FromCacheHint(cacheHint);
    }

    public PyDataType GetTutorialInfo(PyInteger tutorialID, CallInformation call)
    {
        return this.DB.GetTutorialInfo(tutorialID);
    }

    public PyDataType GetCriterias(CallInformation call)
    {
        this.CacheStorage.Load(
            "tutorialSvc",
            "GetCriterias",
            "SELECT criteriaID, criteriaName, messageText, audioPath, 0 AS dataID FROM tutorial_criteria",
            CacheStorage.CacheObjectType.CRowset
        );

        PyDataType cacheHint = this.CacheStorage.GetHint("tutorialSvc", "GetCriterias");

        return CachedMethodCallResult.FromCacheHint(cacheHint);
    }

    public PyDataType GetTutorialGoodies(PyInteger tutorialID, PyInteger pageID, PyInteger pageNumber, CallInformation call)
    {
        // there's not tutorial goodies that we know of
        return new PyList();
    }

    public PyDataType LogStarted(PyInteger       tutorialID, PyInteger pageNumber, PyInteger secondsAfterOpeningTutorial,
                                 CallInformation call)
    {
        // there's no need to log when the tutorial started
        // no interest in this kind of metrics
        return null;
    }

    public PyDataType LogCompleted(PyInteger tutorialID,                  PyInteger       pageNumber,
                                   PyInteger secondsAfterOpeningTutorial, CallInformation call)
    {
        // there's no need to log when the tutorial was completed
        // no interest in this kind of metrics
        return null;
    }

    public PyDataType LogAborted(PyInteger tutorialID,                  PyInteger       pageNumber,
                                 PyInteger secondsAfterOpeningTutorial, CallInformation call)
    {
        // there's no need to log when the tutorial was aborted
        // no interest in this kind of metrics
        return null;
    }

    public PyDataType GetTutorialAgents(PyList agentIDs, CallInformation call)
    {
        return this.DB.GetTutorialAgents(agentIDs.GetEnumerable<PyInteger>());
    }
}
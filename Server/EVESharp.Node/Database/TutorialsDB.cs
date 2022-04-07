using System.Collections.Generic;
using EVESharp.Common.Database;
using EVESharp.PythonTypes.Types.Collections;
using EVESharp.PythonTypes.Types.Database;
using EVESharp.PythonTypes.Types.Primitives;

namespace EVESharp.Node.Database;

public class TutorialsDB : DatabaseAccessor
{
    public TutorialsDB (DatabaseConnection db) : base (db) { }

    private PyDataType GetTutorialPages (int tutorialID)
    {
        // TODO: SUPPORT PAGEACTIONID FIELD, IT'S ONLY USED TO OPEN THE CAREER AGENTS PAGE, BUT WE MIGHT WANT TO DO THAT FOR COMPLETENESS SAKE
        return Database.PrepareCRowsetQuery (
            "SELECT pageID, pageNumber, pageName, text, imagePath, audioPath, 0 AS dataID, NULL AS pageActionID, NULL AS uiPointerID, NULL AS uiPointerText FROM tutorial_pages WHERE tutorialID = @tutorialID ORDER BY pageNumber",
            new Dictionary <string, object> {{"@tutorialID", tutorialID}}
        );
    }

    private PyDataType GetTutorial (int tutorialID)
    {
        return Database.PrepareCRowsetQuery (
            "SELECT tutorialID, tutorialName, nextTutorialID, categoryID, 0 AS dataID FROM tutorials WHERE tutorialID = @tutorialID",
            new Dictionary <string, object> {{"@tutorialID", tutorialID}}
        );
    }

    private PyDataType GetTutorialPageCriterias (int tutorialID)
    {
        return Database.PrepareRowsetQuery (
            "SELECT pageID, criteriaID FROM tutorial_page_criteria LEFT JOIN tutorial_pages USING (pageID) WHERE tutorialID = @tutorialID",
            new Dictionary <string, object> {{"@tutorialID", tutorialID}}
        );
    }

    private PyDataType GetTutorialCriterias (int tutorialID)
    {
        return Database.PrepareRowsetQuery (
            "SELECT criteriaID, criteriaName, messageText, tutorial_criteria.audioPath FROM tutorial_criteria LEFT JOIN tutorial_page_criteria USING (criteriaID) LEFT JOIN tutorial_pages USING (pageID) WHERE tutorialID = @tutorialID",
            new Dictionary <string, object> {{"@tutorialID", tutorialID}}
        );
    }

    public PyDataType GetTutorialInfo (int tutorialID)
    {
        return KeyVal.FromDictionary (
            new PyDictionary
            {
                ["pages"]         = this.GetTutorialPages (tutorialID),
                ["tutorial"]      = this.GetTutorial (tutorialID),
                ["pagecriterias"] = this.GetTutorialPageCriterias (tutorialID),
                ["criterias"]     = this.GetTutorialCriterias (tutorialID)
            }
        );
    }

    public PyDataType GetTutorialAgents (PyList <PyInteger> agentIDs)
    {
        return Database.PrepareCRowsetQuery (
            "SELECT agentID, agentTypeID, divisionID, level, agtAgents.stationID, quality," +
            " agtAgents.corporationID, bloodlineTypes.bloodlineID, chrInformation.gender" +
            " FROM agtAgents" +
            " LEFT JOIN chrInformation on chrInformation.characterID = agtAgents.agentID" +
            " LEFT JOIN invItems ON invItems.itemID = chrInformation.characterID" +
            $" LEFT JOIN bloodlineTypes USING (typeID) WHERE agentID IN ({PyString.Join (',', agentIDs)})"
        );
    }
}
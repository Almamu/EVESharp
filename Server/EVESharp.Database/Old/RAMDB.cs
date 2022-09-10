using System.Collections.Generic;
using EVESharp.EVE.Types;

namespace EVESharp.Database.Old;

public class RAMDB : DatabaseAccessor
{
    public RAMDB (IDatabaseConnection db) : base (db) { }

    public Rowset GetRegionDetails (int regionID)
    {
        // TODO: IS THIS REALLY FETCHED FROM THE STATIONS TABLE?
        return this.Database.PrepareRowset (
            "SELECT" +
            " stationID AS containerID," +
            " stationTypeID AS containerTypeID," +
            " ramAssemblyLineStations.solarSystemID AS containerLocationID," +
            " assemblyLineTypeID," +
            " quantity," +
            " ramAssemblyLineStations.ownerID" +
            " FROM ramAssemblyLineStations" +
            // this left join and the first where clause limit the stations to npc stations
            " LEFT JOIN crpNPCCorporations AS corp ON ramAssemblyLineStations.ownerID = corp.corporationID" +
            " WHERE ramAssemblyLineStations.ownerID = corp.corporationID" +
            " AND ramAssemblyLineStations.regionID = @regionID",
            new Dictionary <string, object> {{"@regionID", regionID}}
        );
    }

    public Rowset GetPersonalDetails (int characterID)
    {
        return this.Database.PrepareRowset (
            "SELECT" +
            " station.stationID AS containerID," +
            " station.stationTypeID AS containerTypeID," +
            " station.solarSystemID AS containerLocationID," +
            " station.assemblyLineTypeID," +
            " station.quantity," +
            " station.ownerID" +
            " FROM ramAssemblyLineStations AS station" +
            " LEFT JOIN ramAssemblyLines AS line ON station.stationID = line.containerID AND station.assemblyLineTypeID = line.assemblyLineTypeID AND station.ownerID = line.ownerID" +
            " WHERE station.ownerID = @characterID" +
            " AND (line.restrictionMask & 12) = 0", // (restrictionMask & (ramRestrictByCorp | ramRestrictByAlliance)) = 0
            new Dictionary <string, object> {{"@characterID", characterID}}
        );
    }

    public Rowset AssemblyLinesGet (int containerID)
    {
        // TODO: CHECK FOR PERMISSIONS FIRST!
        return this.Database.PrepareRowset (
            "SELECT assemblyLineID, assemblyLineTypeID, containerID, nextFreeTime, costInstall, costPerHour, restrictionMask, discountPerGoodStandingPoint, surchargePerBadStandingPoint, minimumStanding, minimumCharSecurity, minimumCorpSecurity, maximumCharSecurity, maximumCorpSecurity FROM ramAssemblyLines WHERE containerID = @containerID",
            new Dictionary <string, object> {{"@containerID", containerID}}
        );
    }

    public Rowset GetJobs2 (int ownerID, bool completed, long fromDate, long toDate)
    {
        return this.Database.PrepareRowset (
            "SELECT" +
            " job.jobID," +
            " job.assemblyLineID," +
            " assemblyLine.containerID," +
            " job.installedItemID," +
            " installedItem.typeID AS installedItemTypeID," +
            " installedItem.ownerID AS installedItemOwnerID," +
            " blueprint.productivityLevel AS installedItemProductivityLevel," +
            " blueprint.materialLevel AS installedItemMaterialLevel," +
            " IF(assemblyLine.activityID = 1, blueprintType.productTypeID, installedItem.typeID) AS outputTypeID," +
            " job.outputFlag," +
            " job.installerID," +
            " assemblyLine.activityID," +
            " job.runs," +
            " job.installTime," +
            " job.beginProductionTime," +
            " job.pauseProductionTime," +
            " job.endProductionTime," +
            " job.completedStatusID != 0 AS completed," +
            " job.licensedProductionRuns," +
            " job.installedInSolarSystemID," +
            " job.completedStatusID AS completedStatus," +
            " station.stationTypeID AS containerTypeID," +
            " station.solarSystemID AS containerLocationID" +
            " FROM ramJobs AS job" +
            " LEFT JOIN invItems AS installedItem ON job.installedItemID = installedItem.itemID" +
            " LEFT JOIN ramAssemblyLines AS assemblyLine ON job.assemblyLineID = assemblyLine.assemblyLineID" +
            " LEFT JOIN invBlueprints AS blueprint ON installedItem.itemID = blueprint.itemID" +
            " LEFT JOIN invBlueprintTypes AS blueprintType ON installedItem.typeID = blueprintType.blueprintTypeID" +
            " LEFT JOIN ramAssemblyLineStations AS station ON assemblyLine.containerID = station.stationID" +
            " WHERE job.ownerID = @ownerID" +
            $" AND job.completedStatusID {(completed ? "!=" : "=")} 0" +
            " AND job.installTime >= @fromDate" +
            " AND job.endProductionTime <= @toDate" +
            " GROUP BY job.jobID",
            new Dictionary <string, object>
            {
                {"@ownerID", ownerID},
                {"@fromDate", fromDate},
                {"@toDate", toDate}
            }
        );
    }
}
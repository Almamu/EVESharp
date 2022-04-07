using System;
using System.Collections.Generic;
using EVESharp.Node.StaticData.Inventory.Station;
using EVESharp.Node.StaticData;
using EVESharp.PythonTypes.Types.Collections;
using EVESharp.PythonTypes.Types.Database;
using EVESharp.PythonTypes.Types.Primitives;
using Type = EVESharp.Node.StaticData.Inventory.Station.Type;

namespace EVESharp.Node.Inventory.Items.Types;

public class Station : ItemInventory
{
    public Information.Station StationInformation { get; }
        
    public Station(Information.Station info) : base(info.Information)
    {
        this.StationInformation = info;
    }

    public Operation                  Operations               => this.StationInformation.Operations;
    public Type                       StationType              => this.StationInformation.Type;
    public Dictionary<int, Character> Guests                   { get; } = new Dictionary<int, Character>();
    public int                        Security                 => this.StationInformation.Security;
    public double                     DockingCostPerVolume     => this.StationInformation.DockingCostPerVolume;
    public double                     MaxShipVolumeDockable    => this.StationInformation.MaxShipVolumeDockable;
    public int                        OfficeRentalCost         => this.StationInformation.OfficeRentalCost;
    public int                        SolarSystemID            => this.LocationID;
    public int                        ConstellationID          => this.StationInformation.ConstellationID;
    public int                        RegionID                 => this.StationInformation.RegionID;
    public double                     ReprocessingEfficiency   => this.StationInformation.ReprocessingEfficiency;
    public double                     ReprocessingStationsTake => this.StationInformation.ReprocessingStationsTake;
    public int                        ReprocessingHangarFlag   => this.StationInformation.ReprocessingHangarFlag;

    public PyDataType GetStationInfo()
    {
        PyDictionary data = new PyDictionary
        {
            ["stationID"]                = this.ID,
            ["security"]                 = this.Security,
            ["dockingCostPerVolume"]     = this.DockingCostPerVolume,
            ["maxShipVolumeDockable"]    = this.MaxShipVolumeDockable,
            ["officeRentalCost"]         = this.OfficeRentalCost,
            ["operationID"]              = this.Operations.OperationID,
            ["stationTypeID"]            = this.Type.ID,
            ["ownerID"]                  = this.OwnerID,
            ["solarSystemID"]            = this.SolarSystemID,
            ["constellationID"]          = this.ConstellationID,
            ["regionID"]                 = this.RegionID,
            ["stationName"]              = this.Name,
            ["x"]                        = this.X,
            ["y"]                        = this.Y,
            ["z"]                        = this.Z,
            ["reprocessingEfficiency"]   = this.ReprocessingEfficiency,
            ["reprocessingStationsTake"] = this.ReprocessingStationsTake,
            ["reprocessingHangarFlag"]   = this.ReprocessingHangarFlag,
            ["description"]              = this.Operations.Description,
            ["serviceMask"]              = this.Operations.ServiceMask
        };

        // TODO: CREATE OBJECTS FOR CONSTELLATION AND REGION ID SO THESE CAN BE FETCHED FROM MEMORY INSTEAD OF DATABASE

        return KeyVal.FromDictionary(data);
    }

    public bool HasService(Service service)
    {
        return (this.Operations.ServiceMask & (int) service) == (int) service;
    }

    public override void Destroy()
    {
        throw new NotImplementedException("Stations cannot be destroyed as they're regarded as static data!");
    }
}
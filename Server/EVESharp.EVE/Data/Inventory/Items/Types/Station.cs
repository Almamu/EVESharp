using System;
using System.Collections.Generic;
using EVESharp.Database.Types;
using EVESharp.EVE.Data.Inventory.Station;
using EVESharp.EVE.Types;
using EVESharp.Types;
using EVESharp.Types.Collections;

namespace EVESharp.EVE.Data.Inventory.Items.Types;

public class Station : ItemInventory
{
    public Database.Inventory.Types.Information.Station StationInformation { get; }

    public Operation                   Operations               => this.StationInformation.Operations;
    public Inventory.Station.Type      StationType              => this.StationInformation.Type;
    public Dictionary <int, Character> Guests                   { get; } = new Dictionary <int, Character> ();
    public int                         Security                 => this.StationInformation.Security;
    public double                      DockingCostPerVolume     => this.StationInformation.DockingCostPerVolume;
    public double                      MaxShipVolumeDockable    => this.StationInformation.MaxShipVolumeDockable;
    public int                         OfficeRentalCost         => this.StationInformation.OfficeRentalCost;
    public int                         SolarSystemID            => this.LocationID;
    public int                         ConstellationID          => this.StationInformation.ConstellationID;
    public int                         RegionID                 => this.StationInformation.RegionID;
    public double                      ReprocessingEfficiency   => this.StationInformation.ReprocessingEfficiency;
    public double                      ReprocessingStationsTake => this.StationInformation.ReprocessingStationsTake;
    public int                         ReprocessingHangarFlag   => this.StationInformation.ReprocessingHangarFlag;

    public Station (Database.Inventory.Types.Information.Station info) : base (info.Information)
    {
        this.StationInformation = info;
    }

    public PyDataType GetStationInfo ()
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

        return KeyVal.FromDictionary (data);
    }

    public bool HasService (Service service)
    {
        return (this.Operations.ServiceMask & (int) service) == (int) service;
    }

    public override void Destroy ()
    {
        throw new NotImplementedException ("Stations cannot be destroyed as they're regarded as static data!");
    }
}
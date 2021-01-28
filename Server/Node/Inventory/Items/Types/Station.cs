using System;
using System.Collections.Generic;
using Node.Data;
using PythonTypes.Types.Database;
using PythonTypes.Types.Primitives;

namespace Node.Inventory.Items.Types
{
    public class Station : ItemInventory
    {
        private StationType mStationType;
        private StationOperations mOperations;
        private Dictionary<int, Character> mGuests;
        
        private int mSecurity;
        private double mDockingCostPerVolume;
        private double mMaxShipVolumeDockable;
        private int mOfficeRentalCost;
        private int mConstellationID;
        private int mRegionID;
        private double mReprocessingEfficiency;
        private double mReprocessingStationsTake;
        private int mReprocessingHangarFlag;
        
        public Station(StationType stationType, StationOperations operations, int security,
            double dockingCostPerVolume, double maxShipVolumeDockable, int officeRentalCost, int constellationId,
            int regionId, double reprocessingEfficiency, double reprocessingStationsTake, int reprocessingHangarFlag,
            ItemEntity from) : base(from)
        {
            this.mStationType = stationType;
            this.mOperations = operations;
            this.mSecurity = security;
            this.mDockingCostPerVolume = dockingCostPerVolume;
            this.mMaxShipVolumeDockable = maxShipVolumeDockable;
            this.mOfficeRentalCost = officeRentalCost;
            this.mConstellationID = constellationId;
            this.mRegionID = regionId;
            this.mReprocessingEfficiency = reprocessingEfficiency;
            this.mReprocessingStationsTake = reprocessingStationsTake;
            this.mReprocessingHangarFlag = reprocessingHangarFlag;
            this.mGuests = new Dictionary<int, Character>();
        }

        public StationOperations Operations => this.mOperations;
        public StationType StationType => this.mStationType;
        public Dictionary<int, Character> Guests => this.mGuests;
        public int Security => mSecurity;
        public double DockingCostPerVolume => mDockingCostPerVolume;
        public double MaxShipVolumeDockable => mMaxShipVolumeDockable;
        public int OfficeRentalCost => mOfficeRentalCost;
        public int SolarSystemID => (int) this.LocationID;
        public int ConstellationID => mConstellationID;
        public int RegionID => mRegionID;
        public double ReprocessingEfficiency => mReprocessingEfficiency;
        public double ReprocessingStationsTake => mReprocessingStationsTake;
        public int ReprocessingHangarFlag => mReprocessingHangarFlag;

        public PyDataType GetStationInfo()
        {
            PyDictionary data = new PyDictionary
            {
                ["stationID"] = this.ID,
                ["security"] = this.Security,
                ["dockingCostPerVolume"] = this.DockingCostPerVolume,
                ["maxShipVolumeDockable"] = this.MaxShipVolumeDockable,
                ["officeRentalCost"] = this.OfficeRentalCost,
                ["operationID"] = this.Operations.OperationID,
                ["stationTypeID"] = this.Type.ID,
                ["ownerID"] = this.OwnerID,
                ["solarSystemID"] = this.SolarSystemID,
                ["constellationID"] = this.ConstellationID,
                ["regionID"] = this.RegionID,
                ["stationName"] = this.Name,
                ["x"] = this.X,
                ["y"] = this.Y,
                ["z"] = this.Z,
                ["reprocessingEfficiency"] = this.ReprocessingEfficiency,
                ["reprocessingStationsTake"] = this.ReprocessingStationsTake,
                ["reprocessingHangarFlag"] = this.ReprocessingHangarFlag,
                ["description"] = this.Operations.Description,
                ["serviceMask"] = this.Operations.ServiceMask
            };

            // TODO: CREATE OBJECTS FOR CONSTELLATION AND REGION ID SO THESE CAN BE FETCHED FROM MEMORY INSTEAD OF DATABASE

            return KeyVal.FromDictionary(data);
        }

        public override void Destroy()
        {
            throw new NotImplementedException("Stations cannot be destroyed as they're regarded as static data!");
        }
    }
}
using System;
using Node.Database;
using Node.Network;
using PythonTypes.Types.Exceptions;
using PythonTypes.Types.Primitives;

namespace Node.Services.Inventory
{
    public class insuranceSvc : BoundService
    {
        private int mStationID = 0;
        private InsuranceDB DB { get; }
        
        public insuranceSvc(InsuranceDB db, BoundServiceManager manager) : base(manager)
        {
            this.DB = db;
        }

        protected insuranceSvc(InsuranceDB db, int stationID, BoundServiceManager manager) : base (manager)
        {
            this.mStationID = stationID;
            this.DB = db;
        }
        
        protected override BoundService CreateBoundInstance(PyDataType objectData)
        {
            return new insuranceSvc(this.DB, objectData as PyInteger, this.BoundServiceManager);
        }

        public PyDataType GetContracts(CallInformation call)
        {
            if (this.mStationID == 0)
            {
                if (call.Client.ShipID == null)
                    throw new CustomError($"The character is not onboard any ship");
                
                return this.DB.GetContractForShip(call.Client.EnsureCharacterIsSelected(), (int) call.Client.ShipID);
            }
            else
            {
                return this.DB.GetContractsForShipsOnStation(call.Client.EnsureCharacterIsSelected(), this.mStationID);
            }
        }

        public PyDataType GetContractForShip(PyInteger itemID, CallInformation call)
        {
            return this.DB.GetContractForShip(call.Client.EnsureCharacterIsSelected(), itemID);
        }

        public PyDataType GetContracts(PyInteger includeCorp, CallInformation call)
        {
            // TODO: IMPLEMENT THIS VERSION OF GET CONTRACTS FOR THE INSURANCE SVC
            return null;
        }
    }
}
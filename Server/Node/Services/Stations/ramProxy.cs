using Common.Services;
using Node.Database;
using Node.Inventory;
using Node.Inventory.Items;
using Node.Inventory.Items.Attributes;
using Node.Inventory.Items.Types;
using Node.Network;
using Node.StaticData.Inventory;
using PythonTypes.Types.Collections;
using PythonTypes.Types.Exceptions;
using PythonTypes.Types.Primitives;

namespace Node.Services.Stations
{
    public class ramProxy : IService
    {
        private ItemFactory ItemFactory { get; }
        private RAMDB DB { get; }
        
        public ramProxy(RAMDB ramDb, ItemFactory itemFactory)
        {
            this.DB = ramDb;
            this.ItemFactory = itemFactory;
        }
        
        public PyDataType GetRelevantCharSkills(CallInformation call)
        {
            Character character = this.ItemFactory.GetItem<Character>(call.Client.EnsureCharacterIsSelected());
            
            // i guess this call fetches skills that affect maximumManufacturingJobCount and maximumResearchJobCount
            return new PyTuple(2)
            {
                // the first part of the dict is not really used by the client, seems to be old code
                [0] = new PyDictionary<PyInteger, PyInteger>
                {
                    [(int) Types.ScientificNetworking] = character.GetSkillLevel(Types.ScientificNetworking),
                    [(int) Types.SupplyChainManagement] = character.GetSkillLevel(Types.SupplyChainManagement)
                },
                // this part contains the actually useful information
                // used to calculate the maximum manufacturing job count and the maximum research job count the character can have
                [1] = new PyDictionary<PyInteger, PyInteger>
                {
                    [(int) Attributes.manufactureSlotLimit] = 1 + character.GetSkillLevel(Types.MassProduction) + character.GetSkillLevel(Types.AdvancedMassProduction),
                    [(int) Attributes.maxLaborotorySlots] = 1 + character.GetSkillLevel(Types.LaboratoryOperation) + character.GetSkillLevel(Types.AdvancedLaboratoryOperation)
                }
            };
        }

        public PyDataType AssemblyLinesSelect(PyString typeFlag, CallInformation call)
        {
            if (typeFlag == "region")
                return this.DB.GetRegionDetails(call.Client.RegionID);
            if (typeFlag == "char")
                return this.DB.GetPersonalDetails(call.Client.EnsureCharacterIsSelected());
            
            // TODO: HANDLE CORP AND ALLIANCE!

            throw new CustomError("Unknown type flag for AssemblyLinesSelect");
        }

        public PyDataType AssemblyLinesGet(PyInteger containerID, CallInformation call)
        {
            return this.DB.AssemblyLinesGet(containerID);
        }

        public PyDataType GetJobs2(PyInteger ownerID, PyBool completed, PyInteger fromDate, PyInteger toDate, CallInformation call)
        {
            if (ownerID != call.Client.EnsureCharacterIsSelected())
                throw new CustomError("Corporation and/or alliance stuff not implemented yet!");
            
            return this.DB.GetJobs2(ownerID, completed, fromDate ?? long.MinValue, toDate ?? long.MaxValue);
        }
    }
}
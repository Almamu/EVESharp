using EVESharp.Database.Inventory.Types;
using EVESharp.Database.Old;
using EVESharp.EVE.Data.Inventory;
using EVESharp.EVE.Data.Inventory.Items.Types;
using EVESharp.EVE.Exceptions;
using EVESharp.EVE.Network.Services;
using EVESharp.EVE.Network.Services.Validators;
using EVESharp.Types;
using EVESharp.Types.Collections;

namespace EVESharp.Node.Services.Stations;

[MustBeCharacter]
public class ramProxy : Service
{
    public override AccessLevel AccessLevel => AccessLevel.None;
    private         IItems      Items       { get; }
    private         RAMDB       DB          { get; }

    public ramProxy (RAMDB ramDb, IItems items)
    {
        DB         = ramDb;
        this.Items = items;
    }

    public PyDataType GetRelevantCharSkills (ServiceCall call)
    {
        Character character = this.Items.GetItem <Character> (call.Session.CharacterID);

        // i guess this call fetches skills that affect maximumManufacturingJobCount and maximumResearchJobCount
        return new PyTuple (2)
        {
            // the first part of the dict is not really used by the client, seems to be old code
            [0] = new PyDictionary <PyInteger, PyInteger>
            {
                [(int) TypeID.ScientificNetworking]  = character.GetSkillLevel (TypeID.ScientificNetworking),
                [(int) TypeID.SupplyChainManagement] = character.GetSkillLevel (TypeID.SupplyChainManagement)
            },
            // this part contains the actually useful information
            // used to calculate the maximum manufacturing job count and the maximum research job count the character can have
            [1] = new PyDictionary <PyInteger, PyInteger>
            {
                [(int) AttributeTypes.manufactureSlotLimit] =
                    1 + character.GetSkillLevel (TypeID.MassProduction) + character.GetSkillLevel (TypeID.AdvancedMassProduction),
                [(int) AttributeTypes.maxLaborotorySlots] = 1 + character.GetSkillLevel (TypeID.LaboratoryOperation) +
                                                            character.GetSkillLevel (TypeID.AdvancedLaboratoryOperation)
            }
        };
    }

    public PyDataType AssemblyLinesSelect (ServiceCall call, PyString typeFlag)
    {
        if (typeFlag == "region")
            return DB.GetRegionDetails (call.Session.RegionID);

        if (typeFlag == "char")
            return DB.GetPersonalDetails (call.Session.CharacterID);

        // TODO: HANDLE CORP AND ALLIANCE!

        throw new CustomError ("Unknown type flag for AssemblyLinesSelect");
    }

    public PyDataType AssemblyLinesGet (ServiceCall call, PyInteger containerID)
    {
        return DB.AssemblyLinesGet (containerID);
    }

    public PyDataType GetJobs2 (ServiceCall call, PyInteger ownerID, PyBool completed, PyInteger fromDate, PyInteger toDate)
    {
        if (ownerID != call.Session.CharacterID)
            throw new CustomError ("Corporation and/or alliance stuff not implemented yet!");

        return DB.GetJobs2 (ownerID, completed, fromDate ?? long.MinValue, toDate ?? long.MaxValue);
    }
}
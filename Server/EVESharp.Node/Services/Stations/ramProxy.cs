using EVESharp.EVE.Packets.Exceptions;
using EVESharp.EVE.Services;
using EVESharp.EVE.Services.Validators;
using EVESharp.EVE.StaticData.Inventory;
using EVESharp.Node.Database;
using EVESharp.Node.Inventory;
using EVESharp.Node.Inventory.Items.Types;
using EVESharp.Node.Sessions;
using EVESharp.PythonTypes.Types.Collections;
using EVESharp.PythonTypes.Types.Primitives;

namespace EVESharp.Node.Services.Stations;

[MustBeCharacter]
public class ramProxy : Service
{
    public override AccessLevel AccessLevel => AccessLevel.None;
    private         ItemFactory ItemFactory { get; }
    private         RAMDB       DB          { get; }

    public ramProxy (RAMDB ramDb, ItemFactory itemFactory)
    {
        DB          = ramDb;
        ItemFactory = itemFactory;
    }

    public PyDataType GetRelevantCharSkills (CallInformation call)
    {
        Character character = ItemFactory.GetItem <Character> (call.Session.CharacterID);

        // i guess this call fetches skills that affect maximumManufacturingJobCount and maximumResearchJobCount
        return new PyTuple (2)
        {
            // the first part of the dict is not really used by the client, seems to be old code
            [0] = new PyDictionary <PyInteger, PyInteger>
            {
                [(int) Types.ScientificNetworking]  = character.GetSkillLevel (Types.ScientificNetworking),
                [(int) Types.SupplyChainManagement] = character.GetSkillLevel (Types.SupplyChainManagement)
            },
            // this part contains the actually useful information
            // used to calculate the maximum manufacturing job count and the maximum research job count the character can have
            [1] = new PyDictionary <PyInteger, PyInteger>
            {
                [(int) AttributeTypes.manufactureSlotLimit] =
                    1 + character.GetSkillLevel (Types.MassProduction) + character.GetSkillLevel (Types.AdvancedMassProduction),
                [(int) AttributeTypes.maxLaborotorySlots] = 1 + character.GetSkillLevel (Types.LaboratoryOperation) +
                                                            character.GetSkillLevel (Types.AdvancedLaboratoryOperation)
            }
        };
    }

    public PyDataType AssemblyLinesSelect (CallInformation call, PyString typeFlag)
    {
        if (typeFlag == "region")
            return DB.GetRegionDetails (call.Session.RegionID);
        if (typeFlag == "char")
            return DB.GetPersonalDetails (call.Session.CharacterID);

        // TODO: HANDLE CORP AND ALLIANCE!

        throw new CustomError ("Unknown type flag for AssemblyLinesSelect");
    }

    public PyDataType AssemblyLinesGet (CallInformation call, PyInteger containerID)
    {
        return DB.AssemblyLinesGet (containerID);
    }

    public PyDataType GetJobs2 (CallInformation call, PyInteger ownerID, PyBool completed, PyInteger fromDate, PyInteger toDate)
    {
        if (ownerID != call.Session.CharacterID)
            throw new CustomError ("Corporation and/or alliance stuff not implemented yet!");

        return DB.GetJobs2 (ownerID, completed, fromDate ?? long.MinValue, toDate ?? long.MaxValue);
    }
}
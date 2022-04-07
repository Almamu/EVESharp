namespace EVESharp.Node.StaticData.Corporation;

public enum CorporationRole : long
{
    /// <summary>
    /// Includes all other roles and can assign any roles (other than the director role) to members of the corporation. This role can only be assigned or removed by the CEO
    /// </summary>
    Director = 1L,
    /// <summary>
    /// Can accept applications from other players to the corporation.
    /// </summary>
    PersonnelManager = 128L,
    /// <summary>
    /// Read access to all corporation wallet logs and the full corporation asset listing, as well as the ability to pay bills. Also has full access to the "Corporation Deliveries" section of the inventory.
    ///
    /// The role does NOT grant take access to corporation wallets, this will have to be assigned separately.
    /// </summary>
    Accountant = 256L,
    /// <summary>
    /// May view the contents of hangars from other corporation members inside NPC stations where the corporation owns an Office. This does not allow the removal of items from those hangars.
    /// 
    /// Viewing corporation member hangars is not available on Upwell Structures.
    /// </summary>
    SecurityOfficer = 512L,
    /// <summary>
    /// Allows management (set up, delivery or cancellation) for corporation jobs, even those installed by other members of the corporation. Also allows the listing and use of all corporation owned blueprints within the "Blueprints" tab of the Industry window, independent of corporation hangar access. Proper "Take" Access to the respective Hangars or containers is still required for any job requiring input materials.
    /// </summary>
    FactoryManager = 1024L,
    /// <summary>
    /// Has access to all management options of Upwell Structures in possession of the corporation, such as editing reinforcement day and time, the structure Biographies as well as profiles used by structures. Also allows deployment of Upwell Structures for the corporation.
    /// </summary>
    StationManager = 2048L,
    /// <summary>
    /// Can access the "Auditing" tab within the "Members" section of the corporation management to review role assignments and removals from corporation members
    /// </summary>
    Auditor = 4096L,
    HangarCanTake1   = 8192L,
    HangarCanTake2   = 16384L,
    HangarCanTake3   = 32768L,
    HangarCanTake4   = 65536L,
    HangarCanTake5   = 131072L,
    HangarCanTake6   = 262144L,
    HangarCanTake7   = 524288L,
    HangarCanQuery1  = 1048576L,
    HangarCanQuery2  = 2097152L,
    HangarCanQuery3  = 4194304L,
    HangarCanQuery4  = 8388608L,
    HangarCanQuery5  = 16777216L,
    HangarCanQuery6  = 33554432L,
    HangarCanQuery7  = 67108864L,
    AccountCanTake1  = 134217728L,
    AccountCanTake2  = 268435456L,
    AccountCanTake3  = 536870912L,
    AccountCanTake4  = 1073741824L,
    AccountCanTake5  = 2147483648L,
    AccountCanTake6  = 4294967296L,
    AccountCanTake7  = 8589934592L,
    AccountCanQuery1 = 17179869184L,
    AccountCanQuery2 = 34359738368L,
    AccountCanQuery3 = 68719476736L,
    AccountCanQuery4 = 137438953472L,
    AccountCanQuery5 = 274877906944L,
    AccountCanQuery6 = 549755813888L,
    AccountCanQuery7 = 1099511627776L,
    /// <summary>
    /// Can deploy and configure Containers and all deployables except Starbases, Upwell Structures and sovereignty structures in space in the name of the corporation
    /// </summary>
    EquipmentConfig = 2199023255552L,
    ContainerCanTake1 = 4398046511104L,
    ContainerCanTake2 = 8796093022208L,
    ContainerCanTake3 = 17592186044416L,
    ContainerCanTake4 = 35184372088832L,
    ContainerCanTake5 = 70368744177664L,
    ContainerCanTake6 = 140737488355328L,
    ContainerCanTake7 = 281474976710656L,
    /// <summary>
    /// Required to be able to rent offices for the corporation. Manually unrenting offices requires the Director or CEO role.
    /// </summary>
    CanRentOffice = 562949953421312L,
    /// <summary>
    /// Required to be able to start manufacturing jobs on behalf of the corporation.
    /// </summary>
    CanRentFactorySlot = 1125899906842624L,
    /// <summary>
    /// Required to be able to start research jobs on behalf of the corporation.
    /// </summary>
    CanRentResearchSlot = 2251799813685248L,
    /// <summary>
    /// Read-only access to all corporation wallet division balances, bills, and to the "Corporation Deliveries" section of the inventory.
    /// </summary>
    JuniorAccountant = 4503599627370496L,
    /// <summary>
    /// Can deploy and configure Starbases and sovereignty structures in space in the name of the corporation. Does not apply to deploying Upwell Stuctures, which require the "Station Manager" role to be deployed.
    /// </summary>
    StarbaseConfig = 9007199254740992L,
    /// <summary>
    /// Has full access to the "Corporation Deliveries" section of the inventory and may create market orders on behalf of the corporation
    /// </summary>
    Trader = 18014398509481984L,
    /// <summary>
    /// Allows setting a message of the Day within the corporation chat channel (also alliance chat channel where applicable)
    /// </summary>
    ChatManager = 36028797018963968L,
    /// <summary>
    /// Can create or accept contracts in the name of the corporation using the corporation wallet for any fees or other costs/profits that may apply. This role is NOT required to accept contracts that were assigned to the corporation, which can be accepted by each member of the corporation in their own name (paying all fees out of their own wallets).
    /// 
    /// This role does not grant access to the Corporation Deliveries section, which would receive items from contracts that were accepted on behalf of the corporation. The Accountant or Trader role is required as well in order to remove items from Corporation Deliveries.
    /// </summary>
    ContractManager = 72057594037927936L,
    /// <summary>
    /// Can take control of Starbase Defenses (Weapon Batteries etc.) of Starbases owned by the corporation. Not required to take control over Upwell Structure defenses, that access is regulated by the structures profile.
    /// </summary>
    InfrastructureTacticalOfficer = 144115188075855872L,
    /// <summary>
    /// Access to the fuel bays and silos of corporation owned Starbases. Not required for Fuel/Fighter/Ammo Bay access of Upwell structures, as that is access is regulated by the structures profile.
    /// </summary>
    StarbaseCaretaker = 288230376151711744L,
    /// <summary>
    /// Can fully manage corporation fittings
    /// </summary>
    FittingManager = 576460752303423488L
}
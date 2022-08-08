namespace EVESharp.EVE.Data.Inventory;

public static class ItemRanges
{
    public const int FACTION_ID_MIN         = 500000;
    public const int FACTION_ID_MAX         = 1000000;
    public const int NPC_CORPORATION_ID_MIN = 1000000;
    public const int NPC_CORPORATION_ID_MAX = 2000000;
    public const int CELESTIAL_ID_MIN       = 40000000;
    public const int CELESTIAL_ID_MAX       = 50000000;
    public const int SOLARSYSTEM_ID_MIN     = 30000000;
    public const int SOLARSYSTEM_ID_MAX     = 40000000;
    public const int STATION_ID_MIN         = 60000000;
    public const int STATION_ID_MAX         = 70000000;
    public const int NPC_CHARACTER_ID_MIN   = 10000;
    public const int NPC_CHARACTER_ID_MAX   = 100000000;
    public const int USERGENERATED_ID_MIN   = 100000000;

    public static bool IsFactionID (int itemID)
    {
        return itemID >= FACTION_ID_MIN && itemID < FACTION_ID_MAX;
    }
    
    public static bool IsStationID (int itemID)
    {
        return itemID >= STATION_ID_MIN && itemID < STATION_ID_MAX;
    }

    public static bool IsSolarSystemID (int itemID)
    {
        return itemID >= SOLARSYSTEM_ID_MIN && itemID < SOLARSYSTEM_ID_MAX;
    }

    public static bool IsNPCCorporationID (int itemID)
    {
        return itemID >= NPC_CORPORATION_ID_MIN && itemID < NPC_CORPORATION_ID_MAX;
    }

    public static bool IsNPC (int itemID)
    {
        return itemID >= NPC_CHARACTER_ID_MIN && itemID < NPC_CHARACTER_ID_MAX;
    }

    public static bool IsCelestialID (int itemID)
    {
        return itemID >= CELESTIAL_ID_MIN && itemID < CELESTIAL_ID_MAX;
    }

    public static bool IsStaticData (int itemID)
    {
        return itemID < USERGENERATED_ID_MIN;
    }
}
namespace EVESharp.Database.Inventory;

public static class ItemRanges
{
    public static class Factions
    {
        public const int MIN = 500000;
        public const int MAX = 599999;
    }

    public static class NPC
    {
        public const int MIN = 10001;
        public const int MAX = 99999999;
    }

    public static class NPCCorporations
    {
        public const int MIN = 1000000;
        public const int MAX = 1999999;
    }

    public static class Agents
    {
        public const int MIN = 3000000;
        public const int MAX = 3999999;
    }

    public static class Regions
    {
        public const int MIN = 10000000;
        public const int MAX = 19999999;
    }

    public static class Constellations
    {
        public const int MIN = 20000000;
        public const int MAX = 29999999;
    }

    public static class SolarSystems
    {
        public const int MIN = 30000000;
        public const int MAX = 39999999;
    }

    public static class Celestials
    {
        public const int MIN = 40000000;
        public const int MAX = 49999999;
    }

    public static class Stargates
    {
        public const int MIN = 50000000;
        public const int MAX = 59999999;
    }

    public static class Stations
    {
        public const int MIN = 60000000;
        public const int MAX = 69999999;
    }

    public static class Asteroids
    {
        public const int MIN = 70000000;
        public const int MAX = 79999999;
    }

    public static class UserGenerated
    {
        public const int MIN = 100000000;
        public const int MAX = 2099999999;
    }

    public static class FakeItems
    {
        public const int MIN = 2100000000;
    }

    public static bool IsFactionID (int itemID)
    {
        return itemID >= Factions.MIN && itemID <= Factions.MAX;
    }
    
    public static bool IsStationID (int itemID)
    {
        return itemID >= Stations.MIN && itemID <= Stations.MAX;
    }

    public static bool IsSolarSystemID (int itemID)
    {
        return itemID >= SolarSystems.MIN && itemID <= SolarSystems.MAX;
    }

    public static bool IsNPCCorporationID (int itemID)
    {
        return itemID >= NPCCorporations.MIN && itemID <= NPCCorporations.MAX;
    }

    public static bool IsNPC (int itemID)
    {
        return itemID >= NPC.MIN && itemID <= NPC.MAX;
    }

    public static bool IsCelestialID (int itemID)
    {
        return itemID >= Celestials.MIN && itemID <= Celestials.MAX;
    }

    public static bool IsStaticData (int itemID)
    {
        return itemID < UserGenerated.MIN;
    }

    public static bool IsFakeItem (int itemID)
    {
        return itemID >= FakeItems.MIN;
    }
}
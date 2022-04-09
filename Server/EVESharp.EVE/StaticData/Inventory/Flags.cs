namespace EVESharp.EVE.StaticData.Inventory;

public enum Flags
{
    None                          = 0,
    AutoFit                       = 0,
    Wallet                        = 1,
    Factory                       = 2,
    Hangar                        = 4,
    Cargo                         = 5,
    Briefcase                     = 6,
    Skill                         = 7,
    Reward                        = 8,
    Connected                     = 9,
    Disconnected                  = 10,
    LoSlot0                       = 11,
    LoSlot1                       = 12,
    LoSlot2                       = 13,
    LoSlot3                       = 14,
    LoSlot4                       = 15,
    LoSlot5                       = 16,
    LoSlot6                       = 17,
    LoSlot7                       = 18,
    MedSlot0                      = 19,
    MedSlot1                      = 20,
    MedSlot2                      = 21,
    MedSlot3                      = 22,
    MedSlot4                      = 23,
    MedSlot5                      = 24,
    MedSlot6                      = 25,
    MedSlot7                      = 26,
    HiSlot0                       = 27,
    HiSlot1                       = 28,
    HiSlot2                       = 29,
    HiSlot3                       = 30,
    HiSlot4                       = 31,
    HiSlot5                       = 32,
    HiSlot6                       = 33,
    HiSlot7                       = 34,
    FixedSlot                     = 35,
    PromenadeSlot1                = 40,
    PromenadeSlot2                = 41,
    PromenadeSlot3                = 42,
    PromenadeSlot4                = 43,
    PromenadeSlot5                = 44,
    PromenadeSlot6                = 45,
    PromenadeSlot7                = 46,
    PromenadeSlot8                = 47,
    PromenadeSlot9                = 48,
    PromenadeSlot10               = 49,
    PromenadeSlot11               = 50,
    PromenadeSlot12               = 51,
    PromenadeSlot13               = 52,
    PromenadeSlot14               = 53,
    PromenadeSlot15               = 54,
    PromenadeSlot16               = 55,
    Capsule                       = 56,
    Pilot                         = 57,
    Passenger                     = 58,
    BoardingGate                  = 59,
    Crew                          = 60,
    SkillInTraining               = 61,
    CorpMarket                    = 62,
    Locked                        = 63,
    Unlocked                      = 64,
    OfficeSlot1                   = 70,
    OfficeSlot2                   = 71,
    OfficeSlot3                   = 72,
    OfficeSlot4                   = 73,
    OfficeSlot5                   = 74,
    OfficeSlot6                   = 75,
    OfficeSlot7                   = 76,
    OfficeSlot8                   = 77,
    OfficeSlot9                   = 78,
    OfficeSlot10                  = 79,
    OfficeSlot11                  = 80,
    OfficeSlot12                  = 81,
    OfficeSlot13                  = 82,
    OfficeSlot14                  = 83,
    OfficeSlot15                  = 84,
    OfficeSlot16                  = 85,
    Bonus                         = 86,
    DroneBay                      = 87,
    Booster                       = 88,
    Implant                       = 89,
    ShipHangar                    = 90,
    ShipOffline                   = 91,
    RigSlot0                      = 92,
    RigSlot1                      = 93,
    RigSlot2                      = 94,
    RigSlot3                      = 95,
    RigSlot4                      = 96,
    RigSlot5                      = 97,
    RigSlot6                      = 98,
    RigSlot7                      = 99,
    FactoryOperation              = 100,
    CorpSAG2                      = 116,
    CorpSAG3                      = 117,
    CorpSAG4                      = 118,
    CorpSAG5                      = 119,
    CorpSAG6                      = 120,
    CorpSAG7                      = 121,
    SecondaryStorage              = 122,
    CaptainsQuarters              = 123,
    WisPromenade                  = 124,
    SubSystem0                    = 125,
    SubSystem1                    = 126,
    SubSystem2                    = 127,
    SubSystem3                    = 128,
    SubSystem4                    = 129,
    SubSystem5                    = 130,
    SubSystem6                    = 131,
    SubSystem7                    = 132,
    SpecializedFuelBay            = 133,
    SpecializedOreHold            = 134,
    SpecializedGasHold            = 135,
    SpecializedMineralHold        = 136,
    SpecializedSalvageHold        = 137,
    SpecializedShipHold           = 138,
    SpecializedSmallShipHold      = 139,
    SpecializedMediumShipHold     = 140,
    SpecializedLargeShipHold      = 141,
    SpecializedIndustrialShipHold = 142,
    SpecializedAmmoHold           = 143,
    HangarAll                     = 1000,
    Clone                         = 1337,
    Office                        = 1338
}

public static class FlagsExtensions
{
    public static bool IsModule (this Flags value)
    {
        return value.IsHighModule () || value.IsMediumModule () || value.IsLowModule () || value.IsRigModule ();
    }

    public static bool IsHighModule (this Flags value)
    {
        return value == Flags.HiSlot0 || value == Flags.HiSlot1 || value == Flags.HiSlot2 ||
               value == Flags.HiSlot3 || value == Flags.HiSlot4 || value == Flags.HiSlot5 ||
               value == Flags.HiSlot6 || value == Flags.HiSlot7;
    }

    public static bool IsMediumModule (this Flags value)
    {
        return value == Flags.MedSlot0 || value == Flags.MedSlot1 || value == Flags.MedSlot2 ||
               value == Flags.MedSlot3 || value == Flags.MedSlot4 || value == Flags.MedSlot5 ||
               value == Flags.MedSlot6 || value == Flags.MedSlot7;
    }

    public static bool IsLowModule (this Flags value)
    {
        return value == Flags.LoSlot0 || value == Flags.LoSlot1 || value == Flags.LoSlot2 ||
               value == Flags.LoSlot3 || value == Flags.LoSlot4 || value == Flags.LoSlot5 ||
               value == Flags.LoSlot6 || value == Flags.LoSlot7;
    }

    public static bool IsRigModule (this Flags value)
    {
        return value == Flags.RigSlot0 || value == Flags.RigSlot1 || value == Flags.RigSlot2 ||
               value == Flags.RigSlot3 || value == Flags.RigSlot4 || value == Flags.RigSlot5 ||
               value == Flags.RigSlot6 || value == Flags.RigSlot7;
    }
}
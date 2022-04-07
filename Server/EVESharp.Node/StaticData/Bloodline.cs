using EVESharp.Node.StaticData.Inventory;

namespace EVESharp.Node.StaticData;

public class Bloodline
{
    public int    ID                     { get; }
    public Type   CharacterType          { get; }
    public string Name                   { get; }
    public int    RaceID                 { get; }
    public string Description            { get; }
    public string MaleDescription        { get; }
    public string FemaleDescription      { get; }
    public Type   ShipType               { get; }
    public int    CorporationID          { get; }
    public int    Perception             { get; }
    public int    Willpower              { get; }
    public int    Charisma               { get; }
    public int    Memory                 { get; }
    public int    Intelligence           { get; }
    public int    GraphicID              { get; }
    public string ShortDescription       { get; }
    public string ShortMaleDescription   { get; }
    public string ShortFemaleDescription { get; }

    public Bloodline (
        int    bloodlineID,          Type   characterType,     string name,     int raceID,        string description,
        string maleDescription,      string femaleDescription, Type   shipType, int corporationID, int    perception,
        int    willpower,            int    charisma,          int    memory,   int intelligence,  int    graphicID, string shortDescription,
        string shortMaleDescription, string shortFemaleDescription
    )
    {
        ID                     = bloodlineID;
        CharacterType          = characterType;
        Name                   = name;
        RaceID                 = raceID;
        Description            = description;
        MaleDescription        = maleDescription;
        FemaleDescription      = femaleDescription;
        ShipType               = shipType;
        CorporationID          = corporationID;
        Perception             = perception;
        Willpower              = willpower;
        Charisma               = charisma;
        Memory                 = memory;
        Intelligence           = intelligence;
        GraphicID              = graphicID;
        ShortDescription       = shortDescription;
        ShortMaleDescription   = shortMaleDescription;
        ShortFemaleDescription = shortFemaleDescription;
    }
}
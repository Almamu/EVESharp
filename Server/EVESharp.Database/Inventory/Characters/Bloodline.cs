using EVESharp.Database.Inventory.Types;

namespace EVESharp.Database.Inventory.Characters;

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
        this.ID                     = bloodlineID;
        this.CharacterType          = characterType;
        this.Name                   = name;
        this.RaceID                 = raceID;
        this.Description            = description;
        this.MaleDescription        = maleDescription;
        this.FemaleDescription      = femaleDescription;
        this.ShipType               = shipType;
        this.CorporationID          = corporationID;
        this.Perception             = perception;
        this.Willpower              = willpower;
        this.Charisma               = charisma;
        this.Memory                 = memory;
        this.Intelligence           = intelligence;
        this.GraphicID              = graphicID;
        this.ShortDescription       = shortDescription;
        this.ShortMaleDescription   = shortMaleDescription;
        this.ShortFemaleDescription = shortFemaleDescription;
    }
}
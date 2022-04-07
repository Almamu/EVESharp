namespace EVESharp.Node.StaticData;

public class Ancestry
{
    public int       ID               { get; }
    public string    Name             { get; }
    public Bloodline Bloodline        { get; }
    public string    Description      { get; }
    public int       Perception       { get; }
    public int       Willpower        { get; }
    public int       Charisma         { get; }
    public int       Memory           { get; }
    public int       Intelligence     { get; }
    public int       GraphicID        { get; }
    public string    ShortDescription { get; }

    public Ancestry (
        int ancestryId, string name,     Bloodline bloodline, string description,  int perception,
        int willpower,  int    charisma, int       memory,    int    intelligence, int graphicId, string shortDescription
    )
    {
        ID               = ancestryId;
        Name             = name;
        Bloodline        = bloodline;
        Description      = description;
        Perception       = perception;
        Willpower        = willpower;
        Charisma         = charisma;
        Memory           = memory;
        Intelligence     = intelligence;
        GraphicID        = graphicId;
        ShortDescription = shortDescription;
    }
}
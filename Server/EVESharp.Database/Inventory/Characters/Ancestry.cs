namespace EVESharp.Database.Inventory.Characters;

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
        this.ID               = ancestryId;
        this.Name             = name;
        this.Bloodline        = bloodline;
        this.Description      = description;
        this.Perception       = perception;
        this.Willpower        = willpower;
        this.Charisma         = charisma;
        this.Memory           = memory;
        this.Intelligence     = intelligence;
        this.GraphicID        = graphicId;
        this.ShortDescription = shortDescription;
    }
}
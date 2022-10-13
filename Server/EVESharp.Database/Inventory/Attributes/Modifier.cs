using EVESharp.Database.Dogma;

namespace EVESharp.Database.Inventory.Attributes;

public class Modifier
{
    public Association Modification { get; init; }
    public Attribute   Value        { get; init; }
}
using EVESharp.Node.Inventory.Items.Dogma;

namespace EVESharp.Node.Inventory.Items.Attributes;

public class Modifier
{
    public Association Modification { get; init; }
    public Attribute   Value        { get; init; }
}
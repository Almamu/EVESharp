using EVESharp.EVE.Data.Inventory.Items;
using EVESharp.EVE.Data.Inventory.Items.Types;
using EVESharp.EVE.Notifications;
using EVESharp.EVE.Sessions;
using EVESharp.Node.Data.Inventory;

namespace EVESharp.Node.Dogma.Interpreter;

public class Environment
{
    public ItemEntity          Self               { get; init; }
    public Character           Character          { get; init; }
    public Ship                Ship               { get; init; }
    public ItemEntity          Target             { get; init; }
    public Session             Session            { get; init; }
    public IItems              ItemFactory        { get; init; }
    public IDogmaNotifications DogmaNotifications { get; init; }
}
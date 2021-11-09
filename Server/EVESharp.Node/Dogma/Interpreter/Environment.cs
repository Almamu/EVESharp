using EVESharp.Node.Inventory.Items;
using EVESharp.Node.Inventory.Items.Types;
using EVESharp.Node.Network;

namespace EVESharp.Node.Dogma.Interpreter
{
    public class Environment
    {
        public ItemEntity Self { get; init; }
        public Character Character { get; init; }
        public Ship Ship { get; init; }
        public ItemEntity Target { get; init; }
        public Client Client { get; init; }
    }
}
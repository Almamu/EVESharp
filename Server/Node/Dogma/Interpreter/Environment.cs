using Node.Inventory.Items;
using Node.Inventory.Items.Types;
using Node.Network;

namespace Node.Dogma.Interpreter
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
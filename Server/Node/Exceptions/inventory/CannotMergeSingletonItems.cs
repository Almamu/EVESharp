using EVE.Packets.Exceptions;

namespace Node.Exceptions.inventory
{
    public class CannotMergeSingletonItems : UserError
    {
        public CannotMergeSingletonItems() : base("CannotMergeSingletonItems")
        {
        }
    }
}
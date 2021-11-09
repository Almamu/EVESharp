using EVESharp.EVE.Packets.Exceptions;

namespace EVESharp.Node.Exceptions.inventory
{
    public class CanNotUnassembleThisItemType : UserError
    {
        public CanNotUnassembleThisItemType() : base("CanNotUnassembleThisItemType")
        {
        }
    }
}
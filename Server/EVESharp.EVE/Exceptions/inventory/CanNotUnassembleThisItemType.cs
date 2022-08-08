using EVESharp.EVE.Packets.Exceptions;

namespace EVESharp.EVE.Exceptions.inventory;

public class CanNotUnassembleThisItemType : UserError
{
    public CanNotUnassembleThisItemType () : base ("CanNotUnassembleThisItemType") { }
}
using EVESharp.EVE.Packets.Exceptions;

namespace EVESharp.EVE.Client.Exceptions.inventory;

public class CanNotUnassembleThisItemType : UserError
{
    public CanNotUnassembleThisItemType () : base ("CanNotUnassembleThisItemType") { }
}
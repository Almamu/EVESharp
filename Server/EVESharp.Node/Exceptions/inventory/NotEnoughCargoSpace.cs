using EVESharp.EVE.Packets.Exceptions;
using EVESharp.PythonTypes.Types.Collections;

namespace EVESharp.Node.Exceptions.inventory;

public class NotEnoughCargoSpace : UserError
{
    public NotEnoughCargoSpace(double volume, double available) : base("NotEnoughCargoSpace", new PyDictionary {["volume"] = FormatAmount(volume), ["available"] = FormatAmount(available)})
    {
    }
}
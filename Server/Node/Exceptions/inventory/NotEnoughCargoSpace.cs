using EVE.Packets.Exceptions;
using PythonTypes.Types.Collections;

namespace Node.Exceptions.inventory
{
    public class NotEnoughCargoSpace : UserError
    {
        public NotEnoughCargoSpace(double volume, double available) : base("NotEnoughCargoSpace", new PyDictionary {["volume"] = volume, ["available"] = available})
        {
        }
    }
}
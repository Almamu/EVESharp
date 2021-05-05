using EVE.Packets.Exceptions;
using PythonTypes.Types.Collections;

namespace Node.Exceptions.corpRegistry
{
    public class PlayerCantCreateCorporation : UserError
    {
        public PlayerCantCreateCorporation(int cost) : base("PlayerCantCreateCorporation", new PyDictionary {["cost"] = FormatISK (cost)})
        {
        }
    }
}
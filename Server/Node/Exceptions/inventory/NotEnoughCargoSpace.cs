using PythonTypes.Types.Collections;
using PythonTypes.Types.Exceptions;

namespace Node.Exceptions.inventory
{
    public class NotEnoughCargoSpace : UserError
    {
        public NotEnoughCargoSpace(double volume, double available) : base("NotEnoughCargoSpace", new PyDictionary {["volume"] = volume, ["available"] = available})
        {
        }
    }
}
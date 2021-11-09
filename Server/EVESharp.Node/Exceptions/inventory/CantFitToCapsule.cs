using EVESharp.EVE.Packets.Exceptions;
using EVESharp.PythonTypes.Types.Collections;

namespace EVESharp.Node.Exceptions.inventory
{
    public class CantFitToCapsule : UserError
    {
        public CantFitToCapsule() : base("CantFitToCapsule")
        {
        }
    }
}
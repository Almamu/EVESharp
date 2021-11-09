using EVESharp.EVE.Packets.Exceptions;
using EVESharp.PythonTypes.Types.Primitives;

namespace EVESharp.Node.Exceptions.marketProxy
{
    public class MktAsksMustSpecifyItems : UserError
    {
        public MktAsksMustSpecifyItems() : base("MktAsksMustSpecifyItems")
        {
        }
    }
}
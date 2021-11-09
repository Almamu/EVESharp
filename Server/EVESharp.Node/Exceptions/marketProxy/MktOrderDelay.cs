using System;
using EVESharp.EVE.Packets.Exceptions;
using EVESharp.PythonTypes.Types.Collections;
using EVESharp.PythonTypes.Types.Primitives;

namespace EVESharp.Node.Exceptions.marketProxy
{
    public class MktOrderDelay : UserError
    {
        public MktOrderDelay(long delay) : base("MktOrderDelay", new PyDictionary () {["delay"] = FormatShortTime(delay)})
        {
        }
    }
}
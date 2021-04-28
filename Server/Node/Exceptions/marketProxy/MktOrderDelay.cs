using System;
using EVE.Packets.Exceptions;
using PythonTypes.Types.Collections;
using PythonTypes.Types.Primitives;

namespace Node.Exceptions.marketProxy
{
    public class MktOrderDelay : UserError
    {
        public MktOrderDelay(long delay) : base("MktOrderDelay", new PyDictionary () {["delay"] = FormatShortTime(delay)})
        {
        }
    }
}
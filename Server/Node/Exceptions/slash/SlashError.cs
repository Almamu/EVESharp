using EVE.Packets.Exceptions;
using PythonTypes.Types.Collections;
using PythonTypes.Types.Primitives;

namespace Node.Exceptions.slash
{
    public class SlashError : UserError
    {
        public SlashError(string reason) : base("SlashError", new PyDictionary {["reason"] = reason})
        {
        }
    }
}
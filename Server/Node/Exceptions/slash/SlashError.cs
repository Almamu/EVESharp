using PythonTypes.Types.Exceptions;
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
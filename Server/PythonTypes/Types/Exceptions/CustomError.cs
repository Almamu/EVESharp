using PythonTypes.Types.Collections;
using PythonTypes.Types.Network;
using PythonTypes.Types.Primitives;

namespace PythonTypes.Types.Exceptions
{
    /// <summary>
    /// Helper class to represent simple error messages for the client
    /// </summary>
    public class CustomError : UserError
    {
        public CustomError(string error) : base("CustomError", new PyDictionary {["error"] = error})
        {
        }
    }
}
using PythonTypes.Types.Collections;

namespace EVE.Packets.Exceptions
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
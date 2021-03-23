using PythonTypes.Types.Collections;
using PythonTypes.Types.Network;
using PythonTypes.Types.Primitives;

namespace PythonTypes.Types.Exceptions
{
    /// <summary>
    /// Helper class to represent simple error messages for the client
    /// </summary>
    public class CustomError : PyException
    {
        public CustomError(string error) : base("ccp_exceptions.UserError", "CustomError", 
            new PyDictionary ()
            {
                {"error", error},
            }, new PyDictionary())
        {
            this.Keywords["msg"] = this.Reason;
            this.Keywords["dict"] = this.Dictionary;
        }

        public PyDictionary Dictionary => this.Extra as PyDictionary;
    }
}
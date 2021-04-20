using PythonTypes.Types.Collections;
using PythonTypes.Types.Network;

namespace EVE.Packets.Exceptions
{
    /// <summary>
    /// Helper class to work with ccp_exceptions.UserError exceptions
    /// </summary>
    public class UserError : PyException
    {
        public UserError(string type, PyDictionary extra = null) : base("ccp_exceptions.UserError", type, extra, new PyDictionary())
        {
            this.Keywords["msg"] = this.Reason;
            this.Keywords["dict"] = this.Dictionary;
        }

        public PyDictionary Dictionary => this.Extra as PyDictionary;
    }
}
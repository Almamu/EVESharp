using PythonTypes.Types.Network;
using PythonTypes.Types.Primitives;

namespace PythonTypes.Types.Exceptions
{
    public class UserError : PyException
    {
        public UserError(string message) : base("ccp_exceptions.UserError", message, new PyDictionary(), new PyDictionary())
        {
            this.Keywords["msg"] = this.Reason;
            this.Keywords["dict"] = this.Dictionary;
        }

        public PyDictionary Dictionary { get => this.Extra as PyDictionary; }
    }
}
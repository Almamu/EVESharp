using PythonTypes.Types.Exceptions;
using PythonTypes.Types.Primitives;

namespace Node.Exceptions.jumpCloneSvc
{
    public class MktNotOwner : UserError
    {
        public MktNotOwner() : base("MktNotOwner")
        {
        }
    }
}
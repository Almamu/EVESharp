using PythonTypes.Types.Exceptions;

namespace Node.Exceptions.jumpCloneSvc
{
    public class MktNotOwner : UserError
    {
        public MktNotOwner() : base("MktNotOwner")
        {
        }
    }
}
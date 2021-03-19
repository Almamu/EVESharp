using PythonTypes.Types.Exceptions;
using PythonTypes.Types.Primitives;

namespace Node.Exceptions.contractMgr
{
    public class ConDurationZero : UserError
    {
        public ConDurationZero() : base("ConDurationZero")
        {
        }
    }
}
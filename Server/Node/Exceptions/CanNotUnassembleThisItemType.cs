using PythonTypes.Types.Exceptions;
using PythonTypes.Types.Primitives;

namespace Node.Exceptions
{
    public class CanNotUnassembleThisItemType : UserError
    {
        public CanNotUnassembleThisItemType() : base("CanNotUnassembleThisItemType")
        {
        }
    }
}
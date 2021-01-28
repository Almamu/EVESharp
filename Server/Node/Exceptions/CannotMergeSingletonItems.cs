using PythonTypes.Types.Exceptions;
using PythonTypes.Types.Primitives;

namespace Node.Exceptions
{
    public class CannotMergeSingletonItems : UserError
    {
        public CannotMergeSingletonItems() : base("CannotMergeSingletonItems")
        {
        }
    }
}
using PythonTypes.Types.Exceptions;

namespace Node.Exceptions
{
    public class CannotMergeSingletonItems : UserError
    {
        public CannotMergeSingletonItems() : base("CannotMergeSingletonItems")
        {
        }
    }
}
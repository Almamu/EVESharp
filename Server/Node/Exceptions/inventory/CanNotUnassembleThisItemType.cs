using PythonTypes.Types.Exceptions;

namespace Node.Exceptions.inventory
{
    public class CanNotUnassembleThisItemType : UserError
    {
        public CanNotUnassembleThisItemType() : base("CanNotUnassembleThisItemType")
        {
        }
    }
}
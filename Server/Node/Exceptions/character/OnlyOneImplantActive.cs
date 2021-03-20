using Node.Inventory.Items;
using PythonTypes.Types.Collections;
using PythonTypes.Types.Exceptions;
using PythonTypes.Types.Primitives;

namespace Node.Exceptions.character
{
    public class OnlyOneImplantActive : UserError
    {
        public OnlyOneImplantActive(ItemEntity implant) : base("OnlyOneImplantActive", new PyDictionary() { ["typeName" ] = implant.Name})
        {
        }
    }
}
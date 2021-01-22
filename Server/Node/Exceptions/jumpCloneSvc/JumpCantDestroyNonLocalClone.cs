using PythonTypes.Types.Exceptions;
using PythonTypes.Types.Primitives;

namespace Node.Exceptions.jumpCloneSvc
{
    public class JumpCantDestroyNonLocalClone : UserError
    {
        public JumpCantDestroyNonLocalClone() : base("JumpCantDestroyNonLocalClone")
        {
        }
    }
}
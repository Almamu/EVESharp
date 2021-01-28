using PythonTypes.Types.Exceptions;

namespace Node.Exceptions.jumpCloneSvc
{
    public class JumpCantDestroyNonLocalClone : UserError
    {
        public JumpCantDestroyNonLocalClone() : base("JumpCantDestroyNonLocalClone")
        {
        }
    }
}
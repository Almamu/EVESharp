namespace EVESharp.Node.Exceptions
{
    public class LSCCannotAccessControl : LSCStandardException
    {
        public LSCCannotAccessControl(string message) : base("LSCCannotAccessControl", message)
        {
        }
    }
}
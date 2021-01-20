namespace Node.Exceptions
{
    public class LSCCannotDestroy : LSCStandardException
    {
        public LSCCannotDestroy(string message) : base("LSCCannotDestroy", message)
        {
        }
    }
}
namespace Node.Exceptions
{
    public class LSCCannotJoin : LSCStandardException
    {
        public LSCCannotJoin(string message) : base("LSCCannotJoin", message)
        {
        }
    }
}
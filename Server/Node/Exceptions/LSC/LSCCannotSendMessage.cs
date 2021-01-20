namespace Node.Exceptions
{
    public class LSCCannotSendMessage : LSCStandardException
    {
        public LSCCannotSendMessage(string message) : base("LSCCannotSendMessage", message)
        {
        }
    }
}
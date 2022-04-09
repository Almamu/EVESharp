namespace EVESharp.Node.Exceptions.LSC;

public class LSCCannotSendMessage : LSCStandardException
{
    public LSCCannotSendMessage (string message) : base ("LSCCannotSendMessage", message) { }
}
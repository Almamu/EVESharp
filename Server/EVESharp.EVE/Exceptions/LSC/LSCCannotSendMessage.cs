namespace EVESharp.EVE.Exceptions.LSC;

public class LSCCannotSendMessage : LSCStandardException
{
    public LSCCannotSendMessage (string message) : base ("LSCCannotSendMessage", message) { }
}
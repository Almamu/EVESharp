namespace EVESharp.EVE.Client.Exceptions.LSC;

public class LSCCannotSendMessage : LSCStandardException
{
    public LSCCannotSendMessage (string message) : base ("LSCCannotSendMessage", message) { }
}
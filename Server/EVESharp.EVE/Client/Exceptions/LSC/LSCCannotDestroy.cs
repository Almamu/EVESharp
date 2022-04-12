namespace EVESharp.EVE.Client.Exceptions.LSC;

public class LSCCannotDestroy : LSCStandardException
{
    public LSCCannotDestroy (string message) : base ("LSCCannotDestroy", message) { }
}
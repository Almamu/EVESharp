namespace EVESharp.EVE.Exceptions.LSC;

public class LSCCannotDestroy : LSCStandardException
{
    public LSCCannotDestroy (string message) : base ("LSCCannotDestroy", message) { }
}
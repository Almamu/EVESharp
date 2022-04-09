namespace EVESharp.Node.Exceptions.LSC;

public class LSCCannotDestroy : LSCStandardException
{
    public LSCCannotDestroy (string message) : base ("LSCCannotDestroy", message) { }
}
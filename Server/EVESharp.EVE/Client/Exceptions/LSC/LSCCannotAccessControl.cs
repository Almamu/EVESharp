namespace EVESharp.EVE.Client.Exceptions.LSC;

public class LSCCannotAccessControl : LSCStandardException
{
    public LSCCannotAccessControl (string message) : base ("LSCCannotAccessControl", message) { }
}
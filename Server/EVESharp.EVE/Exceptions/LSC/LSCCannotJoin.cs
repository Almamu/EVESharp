namespace EVESharp.EVE.Exceptions.LSC;

public class LSCCannotJoin : LSCStandardException
{
    public LSCCannotJoin (string message) : base ("LSCCannotJoin", message) { }
}
namespace EVESharp.EVE.Data.Chat;

public class Roles
{
    public const int CREATOR           = 8 + 4 + 2 + 1;
    public const int OPERATOR          = 4 + SPEAKER + LISTENER;
    public const int CONVERSATIONALIST = SPEAKER + LISTENER;
    public const int SPEAKER           = 2;
    public const int LISTENER          = 1;
    public const int NOTSPECIFIED      = -1;
}
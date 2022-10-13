namespace EVESharp.Database.Corporations;

public static class CorporationRoleExtensions
{
    public static bool Is (this CorporationRole role, long value)
    {
        long longRole = (long) role;

        return (value & longRole) == longRole;
    }
}
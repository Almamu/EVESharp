using System;

namespace EVESharp.EVE.Services;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public class RequiredRole : Attribute
{
    public ulong Role { get; init; }

    public RequiredRole(ulong role)
    {
        this.Role = role;
    }

    public RequiredRole(Roles role)
    {
        this.Role = (ulong) role;
    }
}
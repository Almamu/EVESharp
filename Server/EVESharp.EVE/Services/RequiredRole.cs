using System;
using EVESharp.EVE.Account;

namespace EVESharp.EVE.Services;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public class RequiredRole : Attribute
{
    public ulong Role { get; init; }

    public RequiredRole(ulong role)
    {
        Role = role;
    }

    public RequiredRole(Roles role)
    {
        Role = (ulong) role;
    }
}
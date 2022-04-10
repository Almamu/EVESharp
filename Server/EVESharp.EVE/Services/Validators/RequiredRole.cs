using System;
using EVESharp.EVE.Account;
using EVESharp.EVE.Sessions;

namespace EVESharp.EVE.Services.Validators;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public class RequiredRole : CallValidator
{
    public ulong Role { get; init; }
    
    public RequiredRole(ulong role, Type exception = null)
    {
        Role      = role;
        Exception = exception;
    }

    public RequiredRole(Roles role, Type exception = null)
    {
        Role      = (ulong) role;
        Exception = exception;
    }
    
    public override bool Validate (Session session)
    {
        return (session.Role & Role) == Role;
    }
}
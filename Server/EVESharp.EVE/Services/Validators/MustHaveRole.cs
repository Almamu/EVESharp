using System;
using EVESharp.EVE.Account;
using EVESharp.EVE.Sessions;

namespace EVESharp.EVE.Services.Validators;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true)]
public class MustHaveRole : CallValidator
{
    public ulong Role { get; init; }
    
    public MustHaveRole(ulong role, Type exception = null)
    {
        Role      = role;
        Exception = exception;
    }

    public MustHaveRole(Roles role, Type exception = null)
    {
        Role      = (ulong) role;
        Exception = exception;
    }
    
    public override bool Validate (Session session)
    {
        return (session.Role & Role) == Role;
    }
}
using System;
using EVESharp.Database.Account;
using EVESharp.EVE.Sessions;

namespace EVESharp.EVE.Network.Services.Validators;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true)]
public class MustHaveRole : CallValidator
{
    public ulong Role { get; init; }
    
    public MustHaveRole(ulong role, Type exception = null)
    {
        this.Role      = role;
        this.Exception = exception;
    }

    public MustHaveRole(Roles role, Type exception = null)
    {
        this.Role      = (ulong) role;
        this.Exception = exception;
    }
    
    public override bool Validate (Session session)
    {
        return (session.Role & this.Role) == this.Role;
    }
}
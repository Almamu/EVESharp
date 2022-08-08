using System;
using System.Linq;
using EVESharp.EVE.Data.Corporation;
using EVESharp.EVE.Data.Messages;
using EVESharp.EVE.Exceptions.corpRegistry;
using EVESharp.EVE.Sessions;

namespace EVESharp.EVE.Services.Validators;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true)]
public class MustHaveCorporationRole : CallValidator
{
    public long[] Roles { get; init; }
    
    public MustHaveCorporationRole(long role)
    {
        Roles               = new [] {role};
        Exception           = typeof (CrpAccessDenied);
        ExceptionParameters = new object [] {MLS.UI_GENERIC_ACCESSDENIED};
    }

    public MustHaveCorporationRole (CorporationRole role)
        : this ((long) role)
    {
    }

    public MustHaveCorporationRole (params long [] roles)
    {
        Roles               = roles;
        Exception           = typeof (CrpAccessDenied);
        ExceptionParameters = new object [] {MLS.UI_GENERIC_ACCESSDENIED};
    }

    public MustHaveCorporationRole (params CorporationRole [] roles)
        : this (roles.Select (x => (long) x).ToArray ())
    {
    }
    
    public MustHaveCorporationRole(long role, Type exception)
    {
        Roles               = new [] {role};
        Exception           = exception;
        ExceptionParameters = new object [] { };
    }

    public MustHaveCorporationRole(CorporationRole role, Type exception)
        : this((long) role, exception)
    {
    }

    public MustHaveCorporationRole (long role, string message)
    {
        Roles               = new [] {role};
        Exception           = typeof (CrpAccessDenied);
        ExceptionParameters = new object [] {message};
    }

    public MustHaveCorporationRole (CorporationRole role, string message)
        : this((long) role, message)
    {
    }

    public MustHaveCorporationRole (string message, params long[] roles)
    {
        Roles               = roles;
        Exception           = typeof (CrpAccessDenied);
        ExceptionParameters = new object [] {message};
    }

    public MustHaveCorporationRole (string message, params CorporationRole [] roles)
        : this (message, roles.Select (x => (long) x).ToArray ())
    {
    }

    public MustHaveCorporationRole (Type exception, params long [] roles)
    {
        Roles               = roles;
        Exception           = exception;
        ExceptionParameters = new object [] { };
    }

    public MustHaveCorporationRole (Type exception, params CorporationRole [] roles)
        : this (exception, roles.Select (x => (long) x).ToArray ())
    {
    }

    public override bool Validate (Session session)
    {
        foreach (long role in Roles)
        {
            if ((session.CorporationRole & role) == role)
                return true;
        }

        return false;
    }
}
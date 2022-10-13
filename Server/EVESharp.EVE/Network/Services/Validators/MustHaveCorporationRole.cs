using System;
using System.Linq;
using EVESharp.Database.Corporations;
using EVESharp.EVE.Data.Messages;
using EVESharp.EVE.Exceptions.corpRegistry;
using EVESharp.EVE.Sessions;

namespace EVESharp.EVE.Network.Services.Validators;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true)]
public class MustHaveCorporationRole : CallValidator
{
    public long[] Roles { get; init; }
    
    public MustHaveCorporationRole(long role)
    {
        this.Roles               = new [] {role};
        this.Exception           = typeof (CrpAccessDenied);
        this.ExceptionParameters = new object [] {MLS.UI_GENERIC_ACCESSDENIED};
    }

    public MustHaveCorporationRole (CorporationRole role)
        : this ((long) role)
    {
    }

    public MustHaveCorporationRole (params long [] roles)
    {
        this.Roles               = roles;
        this.Exception           = typeof (CrpAccessDenied);
        this.ExceptionParameters = new object [] {MLS.UI_GENERIC_ACCESSDENIED};
    }

    public MustHaveCorporationRole (params CorporationRole [] roles)
        : this (roles.Select (x => (long) x).ToArray ())
    {
    }
    
    public MustHaveCorporationRole(long role, Type exception)
    {
        this.Roles               = new [] {role};
        this.Exception           = exception;
        this.ExceptionParameters = new object [] { };
    }

    public MustHaveCorporationRole(CorporationRole role, Type exception)
        : this((long) role, exception)
    {
    }

    public MustHaveCorporationRole (long role, string message)
    {
        this.Roles               = new [] {role};
        this.Exception           = typeof (CrpAccessDenied);
        this.ExceptionParameters = new object [] {message};
    }

    public MustHaveCorporationRole (CorporationRole role, string message)
        : this((long) role, message)
    {
    }

    public MustHaveCorporationRole (string message, params long[] roles)
    {
        this.Roles               = roles;
        this.Exception           = typeof (CrpAccessDenied);
        this.ExceptionParameters = new object [] {message};
    }

    public MustHaveCorporationRole (string message, params CorporationRole [] roles)
        : this (message, roles.Select (x => (long) x).ToArray ())
    {
    }

    public MustHaveCorporationRole (Type exception, params long [] roles)
    {
        this.Roles               = roles;
        this.Exception           = exception;
        this.ExceptionParameters = new object [] { };
    }

    public MustHaveCorporationRole (Type exception, params CorporationRole [] roles)
        : this (exception, roles.Select (x => (long) x).ToArray ())
    {
    }

    public override bool Validate (Session session)
    {
        foreach (long role in this.Roles)
        {
            if ((session.CorporationRole & role) == role)
                return true;
        }

        return false;
    }
}
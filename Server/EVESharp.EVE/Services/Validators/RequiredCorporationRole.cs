using System;
using System.Linq;
using EVESharp.EVE.Client.Exceptions.corpRegistry;
using EVESharp.EVE.Client.Messages;
using EVESharp.EVE.Sessions;
using EVESharp.EVE.StaticData.Corporation;

namespace EVESharp.EVE.Services.Validators;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public class RequiredCorporationRole : CallValidator
{
    public long[] Roles { get; init; }
    
    public RequiredCorporationRole(long role)
    {
        Roles = new long [] {role};
    }

    public RequiredCorporationRole (CorporationRole role)
    {
        Roles = new long [] {(long) role};
    }
    
    public RequiredCorporationRole(long role, Type exception)
    {
        Roles               = new long [] {role};
        Exception           = exception;
        ExceptionParameters = new object [] { };
    }

    public RequiredCorporationRole(CorporationRole role, Type exception)
        : this((long) role, exception)
    {
    }

    public RequiredCorporationRole (long role, string message)
    {
        Roles               = new long [] {role};
        Exception           = typeof (CrpAccessDenied);
        ExceptionParameters = new object [] {message};
    }

    public RequiredCorporationRole (CorporationRole role, string message)
        : this((long) role, message)
    {
    }

    public RequiredCorporationRole (string message, params long[] roles)
    {
        Roles               = roles;
        Exception           = typeof (CrpAccessDenied);
        ExceptionParameters = new object [] {message};
    }

    public RequiredCorporationRole (string message, params CorporationRole [] roles)
        : this (message, roles.Select (x => (long) x).ToArray ())
    {
    }

    public RequiredCorporationRole (Type exception, params long [] roles)
    {
        Roles               = roles;
        Exception           = exception;
        ExceptionParameters = new object [] { };
    }

    public RequiredCorporationRole (Type exception, params CorporationRole [] roles)
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
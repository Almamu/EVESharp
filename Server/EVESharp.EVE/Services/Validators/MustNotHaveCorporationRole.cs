using System;
using System.Linq;
using EVESharp.EVE.Data.Corporation;
using EVESharp.EVE.Sessions;

namespace EVESharp.EVE.Services.Validators;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true)]
public class MustNotHaveCorporationRole : MustHaveCorporationRole
{
    public MustNotHaveCorporationRole (long                      role) : base (role) { }
    public MustNotHaveCorporationRole (CorporationRole           role) : base (role) { }
    public MustNotHaveCorporationRole (params long []            roles) : base (roles) { }
    public MustNotHaveCorporationRole (params CorporationRole [] roles) : base (roles) { }
    public MustNotHaveCorporationRole (long                      role,      Type                      exception) : base (role, exception) { }
    public MustNotHaveCorporationRole (CorporationRole           role,      Type                      exception) : base (role, exception) { }
    public MustNotHaveCorporationRole (long                      role,      string                    message) : base (role, message) { }
    public MustNotHaveCorporationRole (CorporationRole           role,      string                    message) : base (role, message) { }
    public MustNotHaveCorporationRole (string                    message,   params long []            roles) : base (message, roles) { }
    public MustNotHaveCorporationRole (string                    message,   params CorporationRole [] roles) : base (message, roles) { }
    public MustNotHaveCorporationRole (Type                      exception, params long []            roles) : base (exception, roles) { }
    public MustNotHaveCorporationRole (Type                                               exception, params CorporationRole [] roles) : base (exception, roles) { }

    public override bool Validate (Session session)
    {
        return !base.Validate (session);
    }
}
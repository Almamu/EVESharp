using System;
using EVESharp.EVE.Exceptions;
using EVESharp.EVE.Sessions;
using EVESharp.Types;

namespace EVESharp.EVE.Network.Services.Validators;

[AttributeUsage (AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true)]
public class MustBeInStation : CallValidator
{
    public MustBeInStation ()
    {
        this.Exception = typeof (CanOnlyDoInStations);
    }
    
    public override bool Validate (Session session)
    {
        return session.TryGetValue (Session.STATION_ID, out PyDataType value) && value is not null;
    }
}
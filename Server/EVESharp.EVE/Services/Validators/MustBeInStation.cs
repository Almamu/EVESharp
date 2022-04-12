using System;
using EVESharp.EVE.Client.Exceptions;
using EVESharp.EVE.Packets.Exceptions;
using EVESharp.EVE.Sessions;
using EVESharp.PythonTypes.Types.Primitives;

namespace EVESharp.EVE.Services.Validators;

[AttributeUsage (AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true)]
public class MustBeInStation : CallValidator
{
    public MustBeInStation ()
    {
        Exception = typeof (CanOnlyDoInStations);
    }
    
    public override bool Validate (Session session)
    {
        return session.TryGetValue (Session.STATION_ID, out PyDataType value) && value is not null;
    }
}
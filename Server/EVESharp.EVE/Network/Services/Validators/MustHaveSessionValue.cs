using System;
using EVESharp.EVE.Sessions;
using EVESharp.Types;

namespace EVESharp.EVE.Network.Services.Validators;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true)]
public class MustHaveSessionValue : CallValidator
{
    public string Key { get; }
    
    public MustHaveSessionValue (string key, Type exception = null)
    {
        this.Key       = key;
        this.Exception = exception;
    }

    public override bool Validate (Session session)
    {
        return session.TryGetValue (this.Key, out PyDataType value) && value is not null;
    }
}
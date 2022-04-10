using System;
using EVESharp.EVE.Sessions;
using EVESharp.PythonTypes.Types.Primitives;

namespace EVESharp.EVE.Services.Validators;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true)]
public class MustHaveSessionValue : CallValidator
{
    public string Key { get; }
    
    public MustHaveSessionValue (string key, Type exception = null)
    {
        Key       = key;
        Exception = exception;
    }

    public override bool Validate (Session session)
    {
        return session.TryGetValue (Key, out PyDataType value) && value is not null;
    }
}
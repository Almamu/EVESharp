using System;
using EVESharp.EVE.Sessions;
using EVESharp.PythonTypes.Types.Primitives;

namespace EVESharp.EVE.Services.Validators;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public class RequiredSession : CallValidator
{
    public string Key { get; }


    public RequiredSession (string key, Type exception = null)
    {
        Key       = key;
        Exception = exception;
    }

    public override bool Validate (Session session)
    {
        return session.TryGetValue (Key, out PyDataType value) && value is not null;
    }
}
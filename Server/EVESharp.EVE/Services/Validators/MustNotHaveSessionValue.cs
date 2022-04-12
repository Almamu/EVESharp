using System;
using EVESharp.EVE.Sessions;
using EVESharp.PythonTypes.Types.Primitives;

namespace EVESharp.EVE.Services.Validators;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true)]
public class MustNotHaveSessionValue : CallValidator
{
    public string Key { get; }


    public MustNotHaveSessionValue (string key, Type exception = null)
    {
        Key       = key;
        Exception = exception;
    }

    public override bool Validate (Session session)
    {
        return session.TryGetValue (Key, out PyDataType value) == false || value is null;
    }
}
using System;
using EVESharp.EVE.Sessions;

namespace EVESharp.EVE.Services.Validators;

/// <summary>
/// Helper interface to declare proper validators for service calls
/// </summary>
public abstract class CallValidator : Attribute
{
    /// <summary>
    /// The exception to throw when the validation fails (if any)
    /// </summary>
    public Type Exception { get;               protected set; }
    /// <summary>
    /// Parameters to give the exception (if any)
    /// </summary>
    public object[] ExceptionParameters { get; protected set; }
    public abstract bool Validate (Session session);
}
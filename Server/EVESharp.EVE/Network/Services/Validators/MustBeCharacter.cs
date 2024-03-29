using System;
using EVESharp.EVE.Exceptions;
using EVESharp.EVE.Sessions;
using EVESharp.Types;

namespace EVESharp.EVE.Network.Services.Validators;

[AttributeUsage (AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true)]
public class MustBeCharacter : CallValidator
{
    public MustBeCharacter ()
    {
        this.Exception           = typeof (CustomError);
        this.ExceptionParameters = new object [] {"NoCharacterSelected"};
    }
    
    public override bool Validate (Session session)
    {
        return session.TryGetValue (Session.CHAR_ID, out PyDataType value) && value is not null;
    }
}
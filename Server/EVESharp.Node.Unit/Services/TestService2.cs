﻿using EVESharp.EVE.Services;
using NUnit.Framework;
using EVESharp.PythonTypes.Types.Primitives;

namespace EVESharp.Common.Unit.Services
{
    public class TestService2 : Service
    {
        public override AccessLevel AccessLevel => AccessLevel.None;
        
        public const int VALUE1 = 50;
        
        public PyDataType AnotherTestCall(PyInteger value1, CallInfo extra)
        {
            Assert.AreEqual(VALUE1, value1.Value);
            Assert.NotNull(extra);
            Assert.IsInstanceOf<CallInfo>(extra);

            return VALUE1;
        }
    }
}
﻿using PythonTypes.Types.Collections;
using PythonTypes.Types.Exceptions;
using PythonTypes.Types.Primitives;

namespace Node.Exceptions
{
    public class LSCStandardException : UserError
    {
        public LSCStandardException(string type, string message) : base(type, new PyDictionary {["msg"] = message})
        {
        }
    }
}
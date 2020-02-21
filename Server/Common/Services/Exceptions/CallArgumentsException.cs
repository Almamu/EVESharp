using System;

namespace Common.Services.Exceptions
{
    public class CallArgumentsException : Exception
    {
        public CallArgumentsException(string parameter, Type type) : base(
            $"Expected parameter {parameter} of type {type}")
        {
            
        }
    }
}
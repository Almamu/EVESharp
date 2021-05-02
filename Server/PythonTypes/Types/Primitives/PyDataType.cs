using System;
using System.Collections.Generic;
using PythonTypes.Types.Collections;

namespace PythonTypes.Types.Primitives
{
    public class PyDataType
    {
        protected bool Equals(PyDataType other)
        {
            return this == other;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((PyDataType) obj);
        }

        public override int GetHashCode()
        {
            throw new System.NotImplementedException();
        }

        protected PyDataType()
        {
        }

        public static implicit operator PyDataType(string str)
        {
            if (str is null)
                return null;

            return new PyString(str);
        }
        
        public static implicit operator PyDataType(ulong value)
        {
            return new PyInteger((long) value);
        }
        
        public static implicit operator PyDataType(long value)
        {
            return new PyInteger(value);
        }
        
        public static implicit operator PyDataType(uint value)
        {
            return new PyInteger(value);
        }

        public static implicit operator PyDataType(int value)
        {
            return new PyInteger(value);
        }

        public static implicit operator PyDataType(ushort value)
        {
            return new PyInteger(value);
        }

        public static implicit operator PyDataType(short value)
        {
            return new PyInteger(value);
        }

        public static implicit operator PyDataType(byte value)
        {
            return new PyInteger(value);
        }

        public static implicit operator PyDataType(sbyte value)
        {
            return new PyInteger(value);
        }

        public static implicit operator PyDataType(long? value)
        {
            if (value is null)
                return null;

            return new PyInteger((long) value);
        }

        public static implicit operator PyDataType(int? value)
        {
            if (value is null)
                return null;

            return new PyInteger((int) value);
        }

        public static implicit operator PyDataType(short? value)
        {
            if (value is null)
                return null;

            return new PyInteger((short) value);
        }

        public static implicit operator PyDataType(byte? value)
        {
            if (value is null)
                return null;

            return new PyInteger((byte) value);
        }

        public static implicit operator PyDataType(sbyte? value)
        {
            if (value is null)
                return null;

            return new PyInteger((sbyte) value);
        }

        public static implicit operator PyDataType(byte[] value)
        {
            if (value is null)
                return null;

            return new PyBuffer(value);
        }

        public static implicit operator PyDataType(float value)
        {
            return new PyDecimal(value);
        }

        public static implicit operator PyDataType(double value)
        {
            return new PyDecimal(value);
        }

        public static implicit operator PyDataType(bool value)
        {
            return new PyBool(value);
        }

        public static implicit operator PyDataType(float? value)
        {
            if (value is null)
                return null;

            return new PyDecimal((float) value);
        }

        public static implicit operator PyDataType(double? value)
        {
            if (value is null)
                return null;

            return new PyDecimal((double) value);
        }

        public static implicit operator PyDataType(bool? value)
        {
            if (value is null)
                return null;

            return new PyBool((bool) value);
        }
        
        public static implicit operator PyDataType(Dictionary<PyDataType, PyDataType> value)
        {
            return new PyDictionary(value);
        }

        public static implicit operator PyDataType(List<PyDataType> value)
        {
            return new PyList(value);
        }
        
        public static bool operator ==(PyDataType left, PyDataType right)
        {
            // check references first
            if (ReferenceEquals(left, right)) return true;
            // if references are not the same, ensure they are not nulls
            if (ReferenceEquals(left, null)) return false;
            if (ReferenceEquals(right, null)) return false;
            if (left.GetType() != right.GetType()) return false;
            
            // they're the same type and have some value, cast and call the proper equals method
            return left switch
            {
                PyInteger integer => integer == (PyInteger) right,
                PyDecimal @decimal => @decimal == (PyDecimal) right,
                PyString @string => @string == (PyString) right,
                PyNone => true,
                PyBool @bool => @bool == (PyBool) right,
                PyBuffer @buffer => @buffer == (PyBuffer) right,
                _ => false
            };
        }

        public static bool operator !=(PyDataType left, PyDataType right)
        {
            return !(left == right);
        }
    }
}
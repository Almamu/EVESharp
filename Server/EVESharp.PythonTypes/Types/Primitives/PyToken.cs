namespace EVESharp.PythonTypes.Types.Primitives
{
    public class PyToken : PyDataType
    {
        public override int GetHashCode()
        {
            return (Token is not null ? Token.GetHashCode() : 0);
        }

        public string Token { get; }
        public int Length => this.Token.Length;

        public PyToken(string token)
        {
            this.Token = token;
        }

        public static implicit operator PyToken(string value)
        {
            return new PyToken(value);
        }

        public static implicit operator string(PyToken value)
        {
            return value.Token;
        }

        public static bool operator ==(PyToken obj, string value)
        {
            if (ReferenceEquals(null, obj) == true)
            {
                if (value is null)
                    return true;

                return false;
            }

            return obj.Token == value;
        }

        public static bool operator !=(PyToken obj, string value)
        {
            return !(obj == value);
        }
    }
}
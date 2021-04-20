namespace PythonTypes.Types.Primitives
{
    public class PyToken : PyDataType
    {
        protected bool Equals(PyToken other)
        {
            return Token == other.Token;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((PyToken) obj);
        }

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
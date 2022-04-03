namespace EVESharp.PythonTypes.Types.Primitives
{
    public class PyToken : PyDataType
    {
        private bool Equals(PyToken other)
        {
            if (ReferenceEquals(null, other)) return false;

            return this.Token.Equals(other.Token);
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

        public static bool operator ==(PyToken left, PyToken right)
        {
            if (ReferenceEquals(left, right)) return true;
            if (ReferenceEquals(null, left)) return false;

            return left.Equals(right);
        }

        public static bool operator !=(PyToken left, PyToken right)
        {
            return !(left == right);
        }
    }
}
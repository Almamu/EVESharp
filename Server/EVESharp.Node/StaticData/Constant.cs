namespace EVESharp.Node.StaticData
{
    public class Constant
    {
        private string mName;
        private long mValue;

        public Constant(string name, long value)
        {
            this.mName = name;
            this.mValue = value;
        }

        public string Name => this.mName;
        public long Value => this.mValue;

        public static implicit operator long(Constant constant)
        {
            return constant.Value;
        }

        public static implicit operator int(Constant constant)
        {
            return (int) constant.Value;
        }
    }
}
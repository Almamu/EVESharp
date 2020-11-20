namespace Node.Data
{
    public class Constant
    {
        private string mName;
        private int mValue;

        public Constant(string name, int value)
        {
            this.mName = name;
            this.mValue = value;
        }

        public string Name => this.mName;
        public int Value => this.mValue;

        public static implicit operator int(Constant constant)
        {
            return constant.Value;
        }
    }
}
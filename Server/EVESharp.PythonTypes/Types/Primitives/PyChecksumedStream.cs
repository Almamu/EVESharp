namespace EVESharp.PythonTypes.Types.Primitives
{
    public class PyChecksumedStream : PyDataType
    {
        public PyDataType Data { get; }

        public PyChecksumedStream(PyDataType data)
        {
            this.Data = data;
        }

        public override int GetHashCode()
        {
            if (this.Data is null)
                return 0;
            
            return this.Data.GetHashCode() + 1;
        }
    }
}
namespace EVESharp.PythonTypes.Types.Primitives
{
    public class PySubStruct : PyDataType
    {
        public PyDataType Definition { get; }

        public PySubStruct(PyDataType definition)
        {
            this.Definition = definition;
        }

        public override int GetHashCode()
        {
            if (this.Definition is null)
                return 0x36851495;
            
            return this.Definition.GetHashCode() ^ 0x36851495;
        }
    }
}
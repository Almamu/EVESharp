namespace EVESharp.PythonTypes.Types.Primitives;

public class PyChecksumedStream : PyDataType
{
    public PyDataType Data { get; }

    public PyChecksumedStream (PyDataType data)
    {
        Data = data;
    }

    public override int GetHashCode ()
    {
        if (Data is null)
            return 0x24521455;

        return Data.GetHashCode () ^ 0x24521455; // some random magic number to spread the hashcode
    }
}
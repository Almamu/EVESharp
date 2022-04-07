namespace EVESharp.PythonTypes.Types.Primitives;

public class PySubStruct : PyDataType
{
    public PyDataType Definition { get; }

    public PySubStruct (PyDataType definition)
    {
        Definition = definition;
    }

    public override int GetHashCode ()
    {
        if (Definition is null)
            return 0x36851495;

        return Definition.GetHashCode () ^ 0x36851495;
    }
}
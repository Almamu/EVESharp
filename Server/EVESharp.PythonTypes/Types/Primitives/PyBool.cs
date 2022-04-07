namespace EVESharp.PythonTypes.Types.Primitives;

public class PyBool : PyDataType
{
    public bool Value { get; }

    public PyBool (bool value)
    {
        Value = value;
    }

    private bool Equals (PyBool other)
    {
        if (ReferenceEquals (null, other)) return false;

        return Value == other.Value;
    }

    public override int GetHashCode ()
    {
        return Value.GetHashCode ();
    }

    public static bool operator == (PyBool left, PyBool right)
    {
        if (ReferenceEquals (left, right)) return true;
        if (ReferenceEquals (null, left)) return false;

        return left.Equals (right);
    }

    public static bool operator != (PyBool left, PyBool right)
    {
        return !(left == right);
    }

    public static bool operator true (PyBool obj)
    {
        return obj.Value;
    }

    public static bool operator false (PyBool obj)
    {
        return !obj.Value;
    }

    public static implicit operator bool (PyBool obj)
    {
        return obj.Value;
    }

    public static implicit operator PyBool (bool value)
    {
        return new PyBool (value);
    }

    public static implicit operator PyInteger (PyBool obj)
    {
        return new PyInteger (obj.Value ? 1 : 0);
    }
}
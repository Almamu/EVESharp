using Marshal;

namespace Marshal.Network
{
    /// <summary>
    /// A class that can be encoded to a PyObject
    /// </summary>
    public interface Encodeable
    {
        PyObject Encode();
    }
}
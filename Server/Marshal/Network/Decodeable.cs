using Marshal;

namespace Marshal.Network
{
    public interface Decodeable
    {
        void Decode(PyObject from);
    }
}
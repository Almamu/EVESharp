using System.Collections;
using PythonTypes.Types.Primitives;

namespace PythonTypes.Types.Collections
{
    public interface IPyListEnumerable<T> : IEnumerable where T : PyDataType
    {
        new IPyListEnumerator<T> GetEnumerator();
    }
}
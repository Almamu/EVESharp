using System.Collections;
using System.Collections.Generic;
using PythonTypes.Types.Primitives;

namespace PythonTypes.Types.Collections
{
    public interface IPyListEnumerable<T> : IEnumerable<T> where T : PyDataType
    {
        new IPyListEnumerator<T> GetEnumerator();
    }
}
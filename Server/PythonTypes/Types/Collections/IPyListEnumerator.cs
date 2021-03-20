using System.Collections.Generic;
using PythonTypes.Types.Primitives;

namespace PythonTypes.Types.Collections
{
    public interface IPyListEnumerator<T> : IEnumerator<T> where T : PyDataType
    {
    }
}
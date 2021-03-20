using System.Collections.Generic;
using PythonTypes.Types.Primitives;

namespace PythonTypes.Types.Collections
{
    public interface IPyDictionaryEnumerator<TKey, TValue> : IEnumerator<PyDictionaryKeyValuePair<TKey, TValue>>
        where TKey : PyDataType where TValue : PyDataType
    {
    }
}
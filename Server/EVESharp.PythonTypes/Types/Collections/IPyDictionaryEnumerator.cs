using System.Collections.Generic;
using EVESharp.PythonTypes.Types.Primitives;

namespace EVESharp.PythonTypes.Types.Collections
{
    public interface IPyDictionaryEnumerator<TKey, TValue> : IEnumerator<PyDictionaryKeyValuePair<TKey, TValue>>
        where TKey : PyDataType where TValue : PyDataType
    {
    }
}
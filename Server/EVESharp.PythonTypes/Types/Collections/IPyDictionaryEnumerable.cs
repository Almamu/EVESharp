using System.Collections;
using EVESharp.PythonTypes.Types.Primitives;

namespace EVESharp.PythonTypes.Types.Collections;

public interface IPyDictionaryEnumerable <TKey, TValue> : IEnumerable where TKey : PyDataType where TValue : PyDataType
{
    new IPyDictionaryEnumerator <TKey, TValue> GetEnumerator ();
}
using System.Collections;

namespace EVESharp.Types.Collections;

public interface IPyDictionaryEnumerable <TKey, TValue> : IEnumerable where TKey : PyDataType where TValue : PyDataType
{
    new IPyDictionaryEnumerator <TKey, TValue> GetEnumerator ();
}
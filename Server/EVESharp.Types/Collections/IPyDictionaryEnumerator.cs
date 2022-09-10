using System.Collections.Generic;

namespace EVESharp.Types.Collections;

public interface IPyDictionaryEnumerator <TKey, TValue> : IEnumerator <PyDictionaryKeyValuePair <TKey, TValue>>
    where TKey : PyDataType where TValue : PyDataType { }
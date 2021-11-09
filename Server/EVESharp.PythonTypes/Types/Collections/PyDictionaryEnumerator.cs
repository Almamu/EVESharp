using System.Collections;
using System.Collections.Generic;
using EVESharp.PythonTypes.Types.Primitives;

namespace EVESharp.PythonTypes.Types.Collections
{
    /// <summary>
    /// Custom iterator used when iterating PyDictionaries to allow for an easier access
    /// </summary>
    public class PyDictionaryEnumerator<TKey, TValue> : IPyDictionaryEnumerator<TKey, TValue> where TKey : PyDataType where TValue : PyDataType
    {
        private readonly IEnumerator<KeyValuePair<PyDataType,PyDataType>> mEnumerator;
        
        public PyDictionaryEnumerator(IEnumerator<KeyValuePair<PyDataType,PyDataType>> parent)
        {
            this.mEnumerator = parent;
        }

        public bool MoveNext()
        {
            return this.mEnumerator.MoveNext();
        }

        public void Reset()
        {
            this.mEnumerator.Reset();
        }

        public PyDictionaryKeyValuePair<TKey, TValue> Current => new PyDictionaryKeyValuePair<TKey, TValue>(this.mEnumerator.Current);

        object? IEnumerator.Current => ((IEnumerator) this.mEnumerator).Current;

        public void Dispose()
        {
            this.mEnumerator.Dispose();
        }
    }
}
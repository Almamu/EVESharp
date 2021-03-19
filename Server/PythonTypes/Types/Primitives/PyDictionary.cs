using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace PythonTypes.Types.Primitives
{
    public interface IPyDictionaryEnumerable<TKey, TValue> : IEnumerable where TKey : PyDataType where TValue : PyDataType
    {
        new IPyDictionaryEnumerator<TKey, TValue> GetEnumerator();
    }
    
    public interface IPyDictionaryEnumerator<TKey, TValue> : IEnumerator<PyDictionaryKeyValuePair<TKey, TValue>>
        where TKey : PyDataType where TValue : PyDataType
    {
    }

    /// <summary>
    /// Custom iterator used when iterating PyDictionaries to allow for an easier access
    /// </summary>
    public class PyDictionaryEnumerator<TKey, TValue> : IPyDictionaryEnumerator<TKey, TValue> where TKey : PyDataType where TValue : PyDataType
    {
        private IEnumerator<KeyValuePair<PyDataType,PyDataType>> mEnumerator;
        
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

    public class PyDictionaryKeyValuePair
    {
        protected KeyValuePair<PyDataType, PyDataType> mPair;
        
        public PyDictionaryKeyValuePair(KeyValuePair<PyDataType, PyDataType> pair)
        {
            this.mPair = pair;
        }

        public void Deconstruct(out PyDataType key, out PyDataType value)
        {
            this.mPair.Deconstruct(out PyDataType first, out PyDataType second);

            key = first;
            value = second;
        }

        public PyDataType Key => this.mPair.Key;
        public PyDataType Value => this.mPair.Value;
    }

    /// <summary>
    /// The equivalent to KeyValuePair to write custom deconstruct methods
    /// </summary>
    public class PyDictionaryKeyValuePair<TKey, TValue> : PyDictionaryKeyValuePair where TKey : PyDataType where TValue : PyDataType
    {
        public void Deconstruct(out TKey key, out TValue value)
        {
            this.mPair.Deconstruct(out PyDataType first, out PyDataType second);

            key = first as TKey;
            value = second as TValue;
        }

        public new TKey Key => base.Key as TKey;
        public new TValue Value => base.Value as TValue;

        public PyDictionaryKeyValuePair(KeyValuePair<PyDataType, PyDataType> pair) : base(pair)
        {
        }
    }

    /// <summary>
    /// Special PyDictionary used for iterating normal PyDictionaries
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    public class PyDictionary<TKey, TValue> : PyDictionary, IPyDictionaryEnumerable<TKey, TValue> where TKey : PyDataType where TValue : PyDataType
    {
        public PyDictionary() : base()
        {
        }

        public PyDictionary(Dictionary<PyDataType, PyDataType> seed) : base(seed)
        {
        }

        public IPyDictionaryEnumerator<TKey, TValue> GetEnumerator()
        {
            return new PyDictionaryEnumerator<TKey, TValue>(this.mDictionary.GetEnumerator());
        }
    }
    
    public class PyDictionary : PyDataType, IPyDictionaryEnumerable<PyDataType, PyDataType>
    {
        protected readonly Dictionary<PyDataType, PyDataType> mDictionary;

        public PyDictionary() : base(PyObjectType.Dictionary)
        {
            this.mDictionary = new Dictionary<PyDataType, PyDataType>();
        }

        public PyDictionary(Dictionary<PyDataType, PyDataType> seed) : base(PyObjectType.Dictionary)
        {
            this.mDictionary = seed;
        }

        public bool TryGetValue(PyDataType key, out PyDataType value)
        {
            return this.mDictionary.TryGetValue(key, out value);
        }

        public void Add(PyDataType key, PyDataType value)
        {
            this.mDictionary.Add(key, value);
        }

        public bool Remove(PyDataType key)
        {
            return this.mDictionary.Remove(key);
        }

        public bool ContainsKey(PyDataType key)
        {
            return this.mDictionary.ContainsKey(key);
        }

        public bool ContainsValue(PyDataType value)
        {
            return this.mDictionary.ContainsValue(value);
        }

        public PyDataType this[PyDataType index]
        {
            get => this.mDictionary[index];
            set => this.mDictionary[index] = value;
        }

        public IPyDictionaryEnumerable<T1, T2> GetEnumerable<T1, T2>() where T1 : PyDataType where T2 : PyDataType
        {
            return new PyDictionary<T1, T2>(this.mDictionary);
        }

        public IPyDictionaryEnumerator<PyDataType, PyDataType> GetEnumerator()
        {
            return new PyDictionaryEnumerator<PyDataType, PyDataType>(this.mDictionary.GetEnumerator());
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new PyDictionaryEnumerator<PyDataType, PyDataType>(this.mDictionary.GetEnumerator());
        }

        public int Length => this.mDictionary.Count;
        public int Count => this.Length;
    }
}
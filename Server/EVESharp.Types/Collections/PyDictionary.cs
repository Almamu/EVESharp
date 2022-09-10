using System.Collections;
using System.Collections.Generic;

namespace EVESharp.Types.Collections;

/// <summary>
/// Special PyDictionary used for iterating normal PyDictionaries
/// </summary>
/// <typeparam name="TKey"></typeparam>
/// <typeparam name="TValue"></typeparam>
public class PyDictionary <TKey, TValue> : PyDictionary, IPyDictionaryEnumerable <TKey, TValue> where TKey : PyDataType where TValue : PyDataType
{
    public TValue this [TKey index]
    {
        get => this.mDictionary [index] as TValue;
        set => this.mDictionary [index] = value;
    }

    public PyDictionary () { }

    public PyDictionary (Dictionary <PyDataType, PyDataType> seed) : base (seed) { }

    public new IPyDictionaryEnumerator <TKey, TValue> GetEnumerator ()
    {
        return new PyDictionaryEnumerator <TKey, TValue> (this.mDictionary.GetEnumerator ());
    }
}

public class PyDictionary : PyDataType, IPyDictionaryEnumerable <PyDataType, PyDataType>
{
    protected readonly Dictionary <PyDataType, PyDataType> mDictionary;

    public PyDataType this [PyDataType index]
    {
        get => this.mDictionary [index];
        set => this.mDictionary [index] = value;
    }

    public int Length => this.mDictionary.Count;
    public int Count  => this.Length;

    public PyDictionary ()
    {
        this.mDictionary = new Dictionary <PyDataType, PyDataType> ();
    }

    public PyDictionary (Dictionary <PyDataType, PyDataType> seed)
    {
        this.mDictionary = seed;
    }

    public IPyDictionaryEnumerator <PyDataType, PyDataType> GetEnumerator ()
    {
        return new PyDictionaryEnumerator <PyDataType, PyDataType> (this.mDictionary.GetEnumerator ());
    }

    IEnumerator IEnumerable.GetEnumerator ()
    {
        return new PyDictionaryEnumerator <PyDataType, PyDataType> (this.mDictionary.GetEnumerator ());
    }

    public override int GetHashCode ()
    {
        // a similar implementation to PyTuple to make my life easy
        int length      = this.Count;
        int mult        = 1000003;
        int mul2        = 1000005;
        int currentHash = 0x63521485;

        foreach ((PyDataType key, PyDataType value) in this.mDictionary)
        {
            mult += 52368 + length + length; // shift the multiplier
            int elementHash = key?.GetHashCode () ?? PyNone.HASH_VALUE * mult;
            mul2        += 58212 + length + length; // shift the multiplier
            elementHash ^= (value?.GetHashCode () ?? PyNone.HASH_VALUE * mul2) << 3;
            currentHash =  (currentHash ^ elementHash) * mult;
            mult        += 82520 + length + length; // shift the multiplier
        }

        return currentHash + 97531;
    }

    public bool TryGetValue (PyDataType key, out PyDataType value)
    {
        return this.mDictionary.TryGetValue (key, out value);
    }

    public bool TryGetValue <T> (PyDataType key, out T value) where T : PyDataType
    {
        if (this.TryGetValue (key, out PyDataType tmp))
        {
            value = tmp as T;

            return true;
        }

        value = null;

        return false;
    }

    public void SafeGetValue <T> (PyDataType key, out T value) where T : PyDataType
    {
        if (this.TryGetValue (key, out value) == false)
            throw new KeyNotFoundException ();
    }

    public void Add (PyDataType key, PyDataType value)
    {
        this.mDictionary.Add (key, value);
    }

    public bool Remove (PyDataType key)
    {
        return this.mDictionary.Remove (key);
    }

    public void Clear ()
    {
        this.mDictionary.Clear ();
    }

    public bool ContainsKey (PyDataType key)
    {
        return this.mDictionary.ContainsKey (key);
    }

    public bool ContainsValue (PyDataType value)
    {
        return this.mDictionary.ContainsValue (value);
    }

    public PyDictionary <T1, T2> GetEnumerable <T1, T2> () where T1 : PyDataType where T2 : PyDataType
    {
        return new PyDictionary <T1, T2> (this.mDictionary);
    }
}
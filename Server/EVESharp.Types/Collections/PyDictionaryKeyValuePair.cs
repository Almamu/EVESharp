﻿using System.Collections.Generic;

namespace EVESharp.Types.Collections;

public class PyDictionaryKeyValuePair
{
    protected readonly KeyValuePair <PyDataType, PyDataType> mPair;

    public PyDataType Key   => this.mPair.Key;
    public PyDataType Value => this.mPair.Value;

    public PyDictionaryKeyValuePair (KeyValuePair <PyDataType, PyDataType> pair)
    {
        this.mPair = pair;
    }

    public void Deconstruct (out PyDataType key, out PyDataType value)
    {
        this.mPair.Deconstruct (out PyDataType first, out PyDataType second);

        key   = first;
        value = second;
    }
}

/// <summary>
/// The equivalent to KeyValuePair to write custom deconstruct methods
/// </summary>
public class PyDictionaryKeyValuePair <TKey, TValue> : PyDictionaryKeyValuePair where TKey : PyDataType where TValue : PyDataType
{
    public new TKey   Key   => base.Key as TKey;
    public new TValue Value => base.Value as TValue;

    public PyDictionaryKeyValuePair (KeyValuePair <PyDataType, PyDataType> pair) : base (pair) { }

    public void Deconstruct (out TKey key, out TValue value)
    {
        this.mPair.Deconstruct (out PyDataType first, out PyDataType second);

        key   = first as TKey;
        value = second as TValue;
    }
}
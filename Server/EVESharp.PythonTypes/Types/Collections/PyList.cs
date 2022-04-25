using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using EVESharp.PythonTypes.Types.Database;
using EVESharp.PythonTypes.Types.Primitives;
using MySql.Data.MySqlClient;

namespace EVESharp.PythonTypes.Types.Collections;

public class PyList <T> : PyList, IPyListEnumerable <T> where T : PyDataType
{
    public new T this [int index]
    {
        get => base [index] as T;
        set => base [index] = value;
    }

    public PyList () { }

    public PyList (int capacity) : base (capacity) { }

    public PyList (PyDataType [] data) : base (data) { }

    public PyList (List <PyDataType> seed) : base (seed) { }

    IEnumerator <T> IEnumerable <T>.GetEnumerator ()
    {
        return this.GetEnumerator ();
    }

    public new IPyListEnumerator <T> GetEnumerator ()
    {
        return new PyListEnumerator <T> (this.mList.GetEnumerator ());
    }

    public void Add (T value)
    {
        base.Add (value);
    }

    public static implicit operator PyList <T> (PyDataType [] array)
    {
        return new PyList <T> (array);
    }

    public static implicit operator object [] (PyList <T> original)
    {
        throw new NotSupportedException (
            "This exception means that most likely what you're trying to achieve is not being done automatically by the compiler, please check the method you're passing this object to to ensure you get the real IEnumerable and not a different overload"
        );
    }

    public static PyList <T> FromDataReader (IDatabaseConnection connection, DbDataReader reader)
    {
        PyList <T> result = new PyList <T> ();
        FieldType  type   = connection.GetFieldType (reader, 0);

        while (reader.Read ())
            result.Add (IDatabaseConnection.ObjectFromColumn (reader, type, 0));

        return result;
    }
}

public class PyList : PyDataType, IPyListEnumerable <PyDataType>
{
    protected readonly List <PyDataType> mList;

    public int Count => this.mList.Count;

    public PyDataType this [int index]
    {
        get => this.mList [index];
        set => this.mList [index] = value;
    }

    public PyList ()
    {
        this.mList = new List <PyDataType> ();
    }

    public PyList (int capacity)
    {
        this.mList = new List <PyDataType> (new PyDataType[capacity]);
    }

    public PyList (PyDataType [] data)
    {
        this.mList = new List <PyDataType> (data);
    }

    public PyList (List <PyDataType> seed)
    {
        this.mList = seed;
    }

    IEnumerator <PyDataType> IEnumerable <PyDataType>.GetEnumerator ()
    {
        return this.GetEnumerator ();
    }

    public IPyListEnumerator <PyDataType> GetEnumerator ()
    {
        return new PyListEnumerator <PyDataType> (this.mList.GetEnumerator ());
    }

    IEnumerator IEnumerable.GetEnumerator ()
    {
        return this.GetEnumerator ();
    }

    public override int GetHashCode ()
    {
        // a somewhat similar implementation based on python's
        int value = 0x66241585;

        foreach (PyDataType data in this.mList)
        {
            value |=  data?.GetHashCode () ?? PyNone.HASH_VALUE;
            value <<= 3;
        }

        return value;
    }

    public void Add (PyDataType pyDataType)
    {
        this.mList.Add (pyDataType);
    }

    public void AddRange (IEnumerable <PyDataType> list)
    {
        this.mList.AddRange (list);
    }

    public void Remove (int index)
    {
        this.mList.RemoveAt (index);
    }

    public void Clear ()
    {
        this.mList.Clear ();
    }

    public bool Contains (PyDataType data)
    {
        return this.mList.Contains (data);
    }

    public List <PyDataType>.Enumerator GetIterator ()
    {
        return this.mList.GetEnumerator ();
    }

    public PyList <T> GetEnumerable <T> () where T : PyDataType
    {
        return new PyList <T> (this.mList);
    }
}
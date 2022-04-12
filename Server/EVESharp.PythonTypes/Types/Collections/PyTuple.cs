using System;
using System.Collections;
using System.Collections.Generic;
using EVESharp.PythonTypes.Types.Primitives;

namespace EVESharp.PythonTypes.Types.Collections;

public class PyTuple : PyDataType, IEnumerable <PyDataType>
{
    private readonly PyDataType [] mList;

    public PyDataType this [int index]
    {
        get => this.mList [index];
        set => this.mList [index] = value;
    }

    public int Count => this.mList.Length;

    protected PyTuple (PyDataType [] original)
    {
        this.mList = original;
    }

    public PyTuple (int size)
    {
        this.mList = new PyDataType[size];
    }

    public IEnumerator <PyDataType> GetEnumerator ()
    {
        return ((IEnumerable <PyDataType>) this.mList).GetEnumerator ();
    }

    IEnumerator IEnumerable.GetEnumerator ()
    {
        return this.GetEnumerator ();
    }

    public override int GetHashCode ()
    {
        // a somewhat similar implementation based on python's
        int length      = Count;
        int mult        = 1000003;
        int currentHash = 0x345678;

        foreach (PyDataType data in this.mList)
        {
            int elementHash = data?.GetHashCode () ?? PyNone.HASH_VALUE;
            currentHash =  (currentHash ^ elementHash) * mult;
            mult        += 82520 + length + length; // shift the multiplier
        }

        return currentHash + 97531;
    }

    public bool TryGetValue <T> (int key, out T value) where T : PyDataType
    {
        if (key < this.mList.Length)
        {
            value = this.mList [key] as T;

            return true;
        }

        value = null;

        return false;
    }

    public void CopyTo (PyTuple destination, int sourceIndex, int destinationIndex, int count)
    {
        // perform some boundaries checks to ensure the data fits
        if (
            count + destinationIndex - sourceIndex > destination.Count ||
            sourceIndex > Count ||
            sourceIndex + count > Count ||
            sourceIndex < 0 ||
            destinationIndex < 0)
            throw new IndexOutOfRangeException ("Trying to copy tuple items that would be out of range");

        // copy data over
        Array.Copy (
            this.mList, sourceIndex,
            destination.mList, destinationIndex, count
        );
    }

    public static implicit operator PyTuple (List <PyDataType> data)
    {
        return data == null ? null : new PyTuple (data.ToArray ());
    }

    public static implicit operator PyTuple (PyDataType [] data)
    {
        return new PyTuple (data);
    }
}
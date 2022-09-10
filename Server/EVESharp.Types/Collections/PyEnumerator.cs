using System.Collections;
using System.Collections.Generic;

namespace EVESharp.Types.Collections;

public class PyEnumerator <T> : IPyEnumerator <T> where T : PyDataType
{
    protected readonly IEnumerator <PyDataType> mEnumerator;

    public PyEnumerator (IEnumerator <PyDataType> enumerator)
    {
        this.mEnumerator = enumerator;
    }

    public bool MoveNext ()
    {
        return this.mEnumerator.MoveNext ();
    }

    public void Reset ()
    {
        this.mEnumerator.Reset ();
    }

    public T Current => this.mEnumerator.Current as T;

    object? IEnumerator.Current => this.Current;

    public void Dispose ()
    {
        this.mEnumerator.Dispose ();
    }
}
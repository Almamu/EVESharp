using System.Collections.Generic;

namespace EVESharp.Types.Collections;

public interface IPyEnumerable <T> : IEnumerable <T> where T : PyDataType
{
    public int Count { get; }
    public T this [int index] { get; set; }
    new IPyEnumerator <T> GetEnumerator ();
}
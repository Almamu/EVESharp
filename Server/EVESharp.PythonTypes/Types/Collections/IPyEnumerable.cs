using System.Collections;
using System.Collections.Generic;
using EVESharp.PythonTypes.Types.Primitives;

namespace EVESharp.PythonTypes.Types.Collections;

public interface IPyEnumerable <T> : IEnumerable <T> where T : PyDataType
{
    public int Count { get; }
    public T this [int index] { get; set; }
    new IPyEnumerator <T> GetEnumerator ();
}
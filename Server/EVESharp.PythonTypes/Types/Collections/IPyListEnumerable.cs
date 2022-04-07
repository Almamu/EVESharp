using System.Collections;
using System.Collections.Generic;
using EVESharp.PythonTypes.Types.Primitives;

namespace EVESharp.PythonTypes.Types.Collections;

public interface IPyListEnumerable<T> : IEnumerable<T> where T : PyDataType
{
    new IPyListEnumerator<T> GetEnumerator();
}
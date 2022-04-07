using System.Collections.Generic;
using EVESharp.PythonTypes.Types.Primitives;

namespace EVESharp.PythonTypes.Types.Collections;

public interface IPyListEnumerator<T> : IEnumerator<T> where T : PyDataType
{
}
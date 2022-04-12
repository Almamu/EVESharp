using EVESharp.PythonTypes.Types.Collections;
using EVESharp.PythonTypes.Types.Primitives;

namespace EVESharp.EVE.Sessions;

/// <summary>
/// Helper class to build a delta of session updates
/// </summary>
public class SessionChange : PyDictionary<PyString, PyTuple>
{
    /// <summary>
    /// Adds a change to the dictionary of changes
    /// </summary>
    /// <param name="key">The key that changed</param>
    /// <param name="prev">The previous value</param>
    /// <param name="next">The next value</param>
    public void AddChange(PyString key, PyDataType prev, PyDataType next)
    {
        this[key] = new PyTuple(2) {[0] = prev, [1] = next};
    }
}
using EVESharp.EVE.Types.Network;
using EVESharp.Types;
using EVESharp.Types.Collections;
using NUnit.Framework;
using TestExtensions;

namespace EVESharp.EVE.Unit.Types.Network;

public class PyExceptionTests
{
    private const string TYPE_STR   = "GPSTransportClosed";
    private const string REASON_STR = "Connection lost";
    private const int    EXTRA_INFO = 150;
    
    [Test]
    public void ExceptionWithExtraTest ()
    {
        PyDataType ex  = new PyException (TYPE_STR, REASON_STR, EXTRA_INFO, new PyDictionary ());
        PyObject   obj = PyAssert.Object (ex);
        (PyToken type, PyTuple data, PyDictionary keywords) = PyAssert.Tuple <PyToken, PyTuple, PyDictionary> (obj.Header);
        (PyString reason, PyInteger extra)                  = PyAssert.Tuple <PyString, PyInteger> (data, false, 2);

        PyAssert.Token (type, TYPE_STR);
        PyAssert.Integer (extra, EXTRA_INFO);
        PyAssert.String (reason, REASON_STR);
    }
    
    [Test]
    public void ExceptionWithoutExtraTest ()
    {
        PyDataType ex  = new PyException (TYPE_STR, REASON_STR, null, null);
        PyObject   obj = PyAssert.Object (ex);
        (PyToken type, PyTuple data, PyDictionary keywords) = PyAssert.Tuple <PyToken, PyTuple, PyDictionary> (obj.Header, true, 3);
        PyString reason = PyAssert.Tuple <PyString> (data, false, 1);

        PyAssert.Token (type, TYPE_STR);
        PyAssert.String (reason, REASON_STR);
    }
}
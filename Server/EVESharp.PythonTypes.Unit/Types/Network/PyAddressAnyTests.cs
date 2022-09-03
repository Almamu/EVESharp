using EVESharp.PythonTypes.Types.Collections;
using EVESharp.PythonTypes.Types.Network;
using EVESharp.PythonTypes.Types.Primitives;
using NUnit.Framework;
using TestExtensions;

namespace EVESharp.PythonTypes.Unit.Types.Network;

public class PyAddressAnyTests
{
    private const int    CALL_ID      = 350;
    private const string SERVICE_NAME = "alertSvc";
    private const string ANY_TYPE  = "A";
    
    private (PyString, PyString, PyInteger) CheckMainAddressPart (PyDataType data, bool acceptNulls = true)
    {
        PyTuple client = PyAssert.ObjectData <PyTuple> (data, "macho.MachoAddress", true);
        return PyAssert.Tuple <PyString, PyString, PyInteger> (client, acceptNulls, 3);
    }

    [Test]
    public void EmptyNodeAddressTest ()
    {
        (PyString type, PyString service, PyInteger callID) = this.CheckMainAddressPart (
            new PyAddressAny (CALL_ID)
        );

        PyAssert.Integer (callID, CALL_ID);
        Assert.IsNull (service);
        PyAssert.String (type, ANY_TYPE);
    }

    [Test]
    public void NodeIDNodeAddressTest ()
    {
        (PyString type, PyString service, PyInteger callID) = this.CheckMainAddressPart (
            new PyAddressAny (CALL_ID, SERVICE_NAME)
        );

        PyAssert.Integer (callID, CALL_ID);
        PyAssert.String (service, SERVICE_NAME);
        PyAssert.String (type, ANY_TYPE);
    }
}
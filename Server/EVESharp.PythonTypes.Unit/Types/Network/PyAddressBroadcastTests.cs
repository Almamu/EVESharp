using EVESharp.PythonTypes.Types.Collections;
using EVESharp.PythonTypes.Types.Network;
using EVESharp.PythonTypes.Types.Primitives;
using NUnit.Framework;
using TestExtensions;

namespace EVESharp.PythonTypes.Unit.Types.Network;

public class PyAddressBroadcastTests
{
    private const string ID_TYPE        = "ownerID";
    private const int    INTEREST_ID    = 150;
    private const string SERVICE_NAME   = "alertSvc";
    private const string BROADCAST_TYPE = "B";

    private (PyString, PyString, PyList, PyString) CheckMainAddressPart (PyDataType data, bool acceptNulls = true)
    {
        PyTuple client = PyAssert.ObjectData <PyTuple> (data, "macho.MachoAddress", true);
        return PyAssert.Tuple <PyString, PyString, PyList, PyString> (client, acceptNulls, 4);
    }

    [Test]
    public void BasicBroadcastAddressTest ()
    {
        (PyString type, PyString service, PyList idsOfInterest, PyString idType) = this.CheckMainAddressPart (
            new PyAddressBroadcast (new PyList (1) {[0] = INTEREST_ID}, ID_TYPE)
        );

        PyAssert.String (idType, ID_TYPE);
        PyAssert.Integer (PyAssert.List <PyInteger> (idsOfInterest), INTEREST_ID);
        Assert.IsNull (service);
        PyAssert.String (type, BROADCAST_TYPE);
    }

    [Test]
    public void ServiceBroadcastAddressTest ()
    {
        (PyString type, PyString service, PyList idsOfInterest, PyString idType) = this.CheckMainAddressPart (
            new PyAddressBroadcast (new PyList (1) {[0] = INTEREST_ID}, ID_TYPE, SERVICE_NAME)
        );

        PyAssert.String (idType, ID_TYPE);
        PyAssert.Integer (PyAssert.List <PyInteger> (idsOfInterest), INTEREST_ID);
        PyAssert.String (service, SERVICE_NAME);
        PyAssert.String (type,    BROADCAST_TYPE);
    }
}
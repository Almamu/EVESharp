using EVESharp.PythonTypes.Types.Collections;
using EVESharp.PythonTypes.Types.Network;
using EVESharp.PythonTypes.Types.Primitives;
using NUnit.Framework;
using TestExtensions;

namespace EVESharp.PythonTypes.Unit.Types.Network;

public class PyAddressClientTests
{
    private const int    CLIENT_ID    = 150;
    private const int    CALL_ID      = 350;
    private const string SERVICE_NAME = "alertSvc";
    private const string CLIENT_TYPE  = "C";
    private const string NODE_TYPE    = "N";
    
    private (PyString, PyInteger, PyInteger, PyString) CheckMainAddressPart (PyDataType data, bool acceptNulls = true)
    {
        PyTuple client = PyAssert.ObjectData <PyTuple> (data, "macho.MachoAddress", true);
        return PyAssert.Tuple <PyString, PyInteger, PyInteger, PyString> (client, acceptNulls, 4);
    }

    [Test]
    public void EmptyClientAddressTest ()
    {
        (PyString type, PyInteger clientID, PyInteger callID, PyString service) = this.CheckMainAddressPart (
            new PyAddressClient ()
        );
        
        Assert.IsNull (clientID);
        Assert.IsNull (callID);
        Assert.IsNull (service);
        PyAssert.String (type, CLIENT_TYPE);
    }

    [Test]
    public void ClientIDClientAddressTest ()
    {
        (PyString type, PyInteger clientID, PyInteger callID, PyString service) = this.CheckMainAddressPart (
            new PyAddressClient (CLIENT_ID)
        );

        PyAssert.Integer (clientID, CLIENT_ID);
        Assert.IsNull (callID);
        Assert.IsNull (service);
        PyAssert.String (type, CLIENT_TYPE);
    }

    [Test]
    public void CallIDClientAddressTest ()
    {
        (PyString type, PyInteger clientID, PyInteger callID, PyString service) = this.CheckMainAddressPart (
            new PyAddressClient (CLIENT_ID, CALL_ID)
        );

        PyAssert.Integer (clientID, CLIENT_ID);
        PyAssert.Integer (callID,   CALL_ID);
        Assert.IsNull (service);
        PyAssert.String (type, CLIENT_TYPE);
    }

    [Test]
    public void ServiceClientAddressTest ()
    {
        (PyString type, PyInteger clientID, PyInteger callID, PyString service) = this.CheckMainAddressPart (
            new PyAddressClient (CLIENT_ID, CALL_ID, SERVICE_NAME), false
        );

        PyAssert.Integer (clientID, CLIENT_ID);
        PyAssert.Integer (callID,   CALL_ID);
        PyAssert.String (service, SERVICE_NAME);
        PyAssert.String (type,    CLIENT_TYPE);
    }
}
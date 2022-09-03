using EVESharp.PythonTypes.Types.Collections;
using EVESharp.PythonTypes.Types.Network;
using EVESharp.PythonTypes.Types.Primitives;
using NUnit.Framework;
using TestExtensions;

namespace EVESharp.PythonTypes.Unit.Types.Network;

public class PyAddressNodeTests
{
    private const int    NODE_ID    = 150;
    private const int    CALL_ID      = 350;
    private const string SERVICE_NAME = "alertSvc";
    private const string NODE_TYPE  = "N";
    
    private (PyString, PyInteger, PyString, PyInteger) CheckMainAddressPart (PyDataType data, bool acceptNulls = true)
    {
        PyTuple client = PyAssert.ObjectData <PyTuple> (data, "macho.MachoAddress", true);
        return PyAssert.Tuple <PyString, PyInteger, PyString, PyInteger> (client, acceptNulls, 4);
    }

    [Test]
    public void EmptyNodeAddressTest ()
    {
        (PyString type, PyInteger nodeID, PyString service, PyInteger callID) = this.CheckMainAddressPart (
            new PyAddressNode ()
        );
        
        Assert.IsNull (nodeID);
        Assert.IsNull (callID);
        Assert.IsNull (service);
        PyAssert.String (type, NODE_TYPE);
    }

    [Test]
    public void NodeIDNodeAddressTest ()
    {
        (PyString type, PyInteger nodeID, PyString service, PyInteger callID) = this.CheckMainAddressPart (
            new PyAddressNode (NODE_ID)
        );

        PyAssert.Integer (nodeID, NODE_ID);
        Assert.IsNull (callID);
        Assert.IsNull (service);
        PyAssert.String (type, NODE_TYPE);
    }

    [Test]
    public void CallIDNodeAddressTest ()
    {
        (PyString type, PyInteger nodeID, PyString service, PyInteger callID) = this.CheckMainAddressPart (
            new PyAddressNode (NODE_ID, CALL_ID)
        );

        PyAssert.Integer (nodeID, NODE_ID);
        PyAssert.Integer (callID,   CALL_ID);
        Assert.IsNull (service);
        PyAssert.String (type, NODE_TYPE);
    }

    [Test]
    public void ServiceNodeAddressTest ()
    {
        (PyString type, PyInteger nodeID, PyString service, PyInteger callID) = this.CheckMainAddressPart (
            new PyAddressNode (NODE_ID, CALL_ID, SERVICE_NAME), false
        );

        PyAssert.Integer (nodeID, NODE_ID);
        PyAssert.Integer (callID,   CALL_ID);
        PyAssert.String (service, SERVICE_NAME);
        PyAssert.String (type,    NODE_TYPE);
    }
}
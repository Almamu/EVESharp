using EVESharp.EVE.Services;
using EVESharp.PythonTypes.Types.Primitives;

namespace EVESharp.Node.Unit.ServiceManagerTests;

public class ExampleService : Service
{
    public override AccessLevel AccessLevel => AccessLevel.None;

    public PyDataType NormalCall (PyInteger first, ServiceCall extra)
    {
        return 0;
    }

    public PyDataType OverridenCall (PyInteger second, ServiceCall extra)
    {
        return 0;
    }

    public PyDataType OverridenCall (PyInteger second, PyInteger number, ServiceCall extra)
    {
        return 1;
    }

    public PyDataType DefaultCall (PyInteger second, PyInteger number = null, ServiceCall extra = null)
    {
        return 0;
    }

    public PyDataType DefaultCall (PyInteger second, PyInteger number, PyInteger third, ServiceCall extra)
    {
        return 1;
    }
}
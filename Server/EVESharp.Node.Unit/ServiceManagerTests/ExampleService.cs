using EVESharp.EVE.Services;
using EVESharp.PythonTypes.Types.Primitives;

namespace EVESharp.Node.Unit.ServiceManagerTests;

public class ExampleService : Service
{
    public override AccessLevel AccessLevel => AccessLevel.None;

    public PyDataType NormalCall (ServiceCall extra, PyInteger first)
    {
        return 0;
    }

    public PyDataType OverridenCall (ServiceCall extra, PyInteger second)
    {
        return 0;
    }

    public PyDataType OverridenCall (ServiceCall extra, PyInteger second, PyInteger number)
    {
        return 1;
    }

    public PyDataType DefaultCall (ServiceCall extra, PyInteger second, PyInteger number = null)
    {
        return 0;
    }

    public PyDataType DefaultCall (ServiceCall extra, PyInteger second, PyInteger number, PyInteger third)
    {
        return 1;
    }

    public PyDataType NamedPayload (ServiceCall extra, PyInteger first, PyInteger ignored = null, PyString name = null)
    {
        if (ignored is not null)
            return ignored;
        if (name is not null)
            return name;

        return null;
    }
}
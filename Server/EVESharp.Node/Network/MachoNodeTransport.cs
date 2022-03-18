using EVESharp.Common.Logging;
using EVESharp.PythonTypes;
using EVESharp.PythonTypes.Types.Primitives;

namespace EVESharp.Node.Network;

public class MachoNodeTransport : MachoTransport
{
    public MachoNodeTransport(MachoTransport source) : base(source)
    {
        this.Socket.SetReceiveCallback(HandlePacket);
    }

    private void HandlePacket(PyDataType data)
    {
        Log.Debug(PrettyPrinter.FromDataType(data));
    }
}
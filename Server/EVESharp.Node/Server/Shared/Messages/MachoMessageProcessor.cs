using EVESharp.EVE.Messages.Processor;
using EVESharp.EVE.Messages.Queue;
using EVESharp.EVE.Network;
using EVESharp.EVE.Network.Messages;

namespace EVESharp.Node.Server.Shared.Messages;

public class MachoMessageProcessor : ThreadedProcessor <MachoMessage>
{
    public MachoMessageProcessor (IMachoNet machoNet, IMessageQueue <MachoMessage> queue) : base (queue)
    {
        // set the message processor
        machoNet.MessageProcessor = this;
        // start it too
        this.Start ();
    }
}
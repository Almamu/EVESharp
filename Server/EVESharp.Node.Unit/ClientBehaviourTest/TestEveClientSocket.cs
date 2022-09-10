using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using EVESharp.EVE.Network.Sockets;
using EVESharp.Types;

namespace EVESharp.Node.Unit.ClientBehaviourTest;

public class TestEveClientSocket : EVESocket
{
    private Queue <PyDataType>        mSentQueue = new Queue <PyDataType> ();
    private event Action <PyDataType> mActionEvent;

    public override string RemoteAddress => "FakeSocket";
    
    public event Action <PyDataType> DataSent
    {
        add
        {
            this.mActionEvent += value;
            
            // check if there's any sent data pending
            while (this.mSentQueue.TryDequeue (out PyDataType data))
                this.mActionEvent (data);
        }

        remove => this.mActionEvent -= value;
    }
    
    public override void Send (PyDataType data)
    {
        if (this.mActionEvent is null)
            this.mSentQueue.Enqueue (data);
        else
            this.mActionEvent (data);
    }
    
    public void SimulateDataReceived (PyDataType data)
    {
        this.OnDataReceived (data);
    }
}
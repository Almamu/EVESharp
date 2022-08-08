using System.Collections.Generic;
using EVESharp.EVE.Packets.Complex;
using EVESharp.PythonTypes.Types.Collections;
using EVESharp.PythonTypes.Types.Primitives;

namespace EVESharp.EVE.Notifications.Inventory;

public class OnModuleAttributeChanges : ClientNotification
{
    private const string NOTITIFATION_NAME = "OnModuleAttributeChanges";

    public PyList Changes { get; }

    public OnModuleAttributeChanges () : base (NOTITIFATION_NAME)
    {
        this.Changes = new PyList ();
    }

    public void AddChange (OnModuleAttributeChange change)
    {
        this.Changes.Add (change);
    }

    public override List <PyDataType> GetElements ()
    {
        return new List <PyDataType> {this.Changes};
    }
}
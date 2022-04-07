using System.Collections.Generic;
using EVESharp.Node.Database;
using EVESharp.Node.Inventory;
using EVESharp.Node.Network;
using EVESharp.Node.Server.Shared;
using EVESharp.Node.StaticData;

namespace EVESharp.Node;

/// <summary>
/// Parent container for the whole program
/// </summary>
public class NodeContainer
{
    /// <summary>
    /// The list of constants for EVE Online
    /// </summary>
    public Dictionary<string, Constant> Constants { get; }

    public NodeContainer(GeneralDB generalDB)
    {
        // load constants for the EVE System
        this.Constants = generalDB.LoadConstants();
    }
}
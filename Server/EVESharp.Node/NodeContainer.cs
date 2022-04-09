using System.Collections.Generic;
using EVESharp.EVE.StaticData;
using EVESharp.Node.Database;

namespace EVESharp.Node;

/// <summary>
/// Parent container for the whole program
/// </summary>
public class NodeContainer
{
    /// <summary>
    /// The list of constants for EVE Online
    /// </summary>
    public Dictionary <string, Constant> Constants { get; }

    public NodeContainer (GeneralDB generalDB)
    {
        // load constants for the EVE System
        Constants = generalDB.LoadConstants ();
    }
}
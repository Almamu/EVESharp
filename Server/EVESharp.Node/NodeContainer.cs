using System.Collections.Generic;
using EVESharp.Node.Database;
using EVESharp.Node.StaticData;

namespace EVESharp.Node
{
    public class NodeContainer
    {
        /// <summary>
        /// The ID assigned to the running node
        /// </summary>
        public long NodeID { get; set; }
        /// <summary>
        /// The address assigned to the running node
        /// </summary>
        public string Address { get; set; }
        /// <summary>
        /// The list of constants for EVE Online
        /// </summary>
        public Dictionary<string, Constant> Constants { get; }

        public NodeContainer(GeneralDB generalDB)
        {
            this.NodeID = 0;
            
            // load constants for the EVE System
            this.Constants = generalDB.LoadConstants();
        }
    }
}
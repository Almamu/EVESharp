using System.Collections.Generic;
using Node.Database;
using Node.StaticData;

namespace Node
{
    public class NodeContainer
    {
        /// <summary>
        /// The ID of the running node
        /// </summary>
        public long NodeID { get; set; }
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
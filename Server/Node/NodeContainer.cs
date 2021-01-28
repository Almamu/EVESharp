using System.Collections.Generic;
using Node.Data;
using Node.Database;

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
        private GeneralDB GeneralDB { get; }

        public NodeContainer(GeneralDB generalDB)
        {
            this.NodeID = 0;
            this.GeneralDB = generalDB;

            // load constants for the EVE System
            this.Constants = this.GeneralDB.LoadConstants();
        }
    }
}
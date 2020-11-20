using System.Collections.Generic;
using Common.Database;
using Common.Logging;
using Node.Data;
using Node.Database;
using Node.Inventory;

namespace Node
{
    public class NodeContainer
    {
        public SystemManager SystemManager { get; set; }
        public ServiceManager ServiceManager { get; set; }
        public ClientManager ClientManager { get; set; }
        public ItemFactory ItemFactory { get; set; }
        public Logger Logger { get; set; }
        public long NodeID { get; set; }
        public DatabaseConnection Database { get; private set; }
        public Dictionary<string, Constant> Constants { get; private set; }

        private readonly GeneralDB mGeneralDB = null;

        public NodeContainer(DatabaseConnection db)
        {
            this.NodeID = 0;
            this.Database = db;
            this.mGeneralDB = new GeneralDB(db);

            // load constants for the EVE System
            this.Constants = this.mGeneralDB.LoadConstants();
        }
    }
}
using Common.Database;
using Common.Logging;
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

        public NodeContainer(DatabaseConnection db)
        {
            this.NodeID = 0;
            this.Database = db;
        }
    }
}
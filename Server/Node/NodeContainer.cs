namespace Node
{
    public class NodeContainer
    {
        public SystemManager SystemManager { get; set; }
        public ServiceManager ServiceManager { get; set; }
        public ClientManager ClientManager { get; set; }
        public long NodeID { get; set; }

        public NodeContainer()
        {
            this.NodeID = 0;
        }
    }
}
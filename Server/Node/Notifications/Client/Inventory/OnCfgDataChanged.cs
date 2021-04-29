using System.Collections.Generic;
using EVE.Packets.Complex;
using Node.Inventory.Items;
using PythonTypes.Types.Collections;
using PythonTypes.Types.Primitives;

namespace Node.Notifications.Client.Inventory
{
    public class OnCfgDataChanged : ClientNotification
    {
        private const string NOTIFICATION_NAME = "OnCfgDataChanged";

        public string What { get; init; }
        public PyDataType Data { get; init; }

        private OnCfgDataChanged(string what, PyDataType data) : base(NOTIFICATION_NAME)
        {
            this.What = what;
            this.Data = data;
        }

        public override List<PyDataType> GetElements()
        {
            return new List<PyDataType>()
            {
                this.What,
                this.Data
            };
        }

        public static OnCfgDataChanged BuildItemLabelChange(ItemEntity item)
        {
            return new OnCfgDataChanged(
                "evelocations",
                new PyList(5)
                {
                    [0] = item.ID,
                    [1] = item.Name,
                    [2] = item.X,
                    [3] = item.Y,
                    [4] = item.Z
                }
            );
        }
    }
}
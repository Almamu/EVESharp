using System.Collections.Generic;
using Node.Inventory.Items;
using PythonTypes.Types.Collections;
using PythonTypes.Types.Complex;
using PythonTypes.Types.Primitives;

namespace Node.Inventory.Notifications
{
    public class OnGodmaShipEffect : PyMultiEventEntry
    {
        public ItemEntity Item { get; init; }
        public int EffectID { get; init; }
        public long Time { get; init; }
        public int ShouldStart { get; init; }
        public int Active { get; init; }
        public int? CharacterID { get; init; }
        public int? ShipID { get; init; }
        public int? Target { get; init; } = null;
        public long StartTime { get; init; }
        public long Duration { get; init; }
        public int Repeat { get; init; }
        public int RandomSeed { get; init; }

        public OnGodmaShipEffect() : base("OnGodmaShipEffect")
        {
        }

        protected override List<PyDataType> GetElements()
        {
            return new List<PyDataType>()
            {
                this.Item.ID,
                this.EffectID,
                this.Time,
                this.ShouldStart,
                this.Active,
                new PyTuple(7)
                {
                    [0] = this.Item.ID,
                    [1] = this.CharacterID,
                    [2] = this.ShipID,
                    [3] = this.Target,
                    [4] = null,
                    [5] = null,
                    [6] = this.EffectID
                },
                this.StartTime,
                this.Duration,
                this.Repeat,
                this.RandomSeed,
                null
            };
        }
    }
}
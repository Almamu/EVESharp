using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EVESharp.Inventory
{
    public class ItemType
    {
        public int typeID { private set; get; }
        public int groupID { private set; get; }
        public string typeName { private set; get; }
        public string description { private set; get; }
        public int graphicID { private set; get; }
        public double radius { private set; get; }
        public double mass { private set; get; }
        public double volume { private set; get; }
        public double capacity { private set; get; }
        public int portionSize { private set; get; }
        public int raceID { private set; get; }
        public double basePrice { private set; get; }
        public bool published { private set; get; }
        public int marketGroupdID { private set; get; }
        public double chanceOfDuplicating { private set; get; }

        public ItemType(int typeID, int groupID, string typeName, string description,
            int graphicID, double radius, double mass, double volume, double capacity,
            int portionSize, int raceID, double basePrice, bool publiches, int marketGroupdID,
            double chanceOfDuplicating)
        {
            this.typeID = typeID;
            this.groupID = groupID;
            this.typeName = typeName;
            this.description = description;
            this.graphicID = graphicID;
            this.radius = radius;
            this.mass = mass;
            this.volume = volume;
            this.capacity = capacity;
            this.portionSize = portionSize;
            this.raceID = raceID;
            this.basePrice = basePrice;
            this.published = published;
            this.marketGroupdID = marketGroupdID;
            this.chanceOfDuplicating = chanceOfDuplicating;
        }
    }
}

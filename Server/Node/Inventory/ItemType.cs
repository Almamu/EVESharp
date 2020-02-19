/*
    ------------------------------------------------------------------------------------
    LICENSE:
    ------------------------------------------------------------------------------------
    This file is part of EVE#: The EVE Online Server Emulator
    Copyright 2012 - Glint Development Group
    ------------------------------------------------------------------------------------
    This program is free software; you can redistribute it and/or modify it under
    the terms of the GNU Lesser General Public License as published by the Free Software
    Foundation; either version 2 of the License, or (at your option) any later
    version.

    This program is distributed in the hope that it will be useful, but WITHOUT
    ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
    FOR A PARTICULAR PURPOSE. See the GNU Lesser General Public License for more details.

    You should have received a copy of the GNU Lesser General Public License along with
    this program; if not, write to the Free Software Foundation, Inc., 59 Temple
    Place - Suite 330, Boston, MA 02111-1307, USA, or go to
    http://www.gnu.org/copyleft/lesser.txt.
    ------------------------------------------------------------------------------------
    Creator: Almamu
*/

namespace Node.Inventory
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
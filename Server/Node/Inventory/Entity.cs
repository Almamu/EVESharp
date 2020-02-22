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

using Node.Database;

namespace Node.Inventory
{
    public class Entity
    {
        public ItemFactory mItemFactory = null;

        public int ID { set; get; }
        public string Name { set; get; }
        public ItemType Type { set; get; }
        public int OwnerID { set; get; } // TODO: CONVERT THIS TO AN ENTITY
        public int LocationID { set; get; } // TODO: CONVERT THIS TO AN ENTITY
        public int Flag { set; get; }
        public bool Contraband { set; get; }
        public bool Singleton { set; get; }
        public int Quantity { set; get; } // TODO: DEPRECATE THIS AND USE QUANTITY ATTRIBUTE
        public double X { set; get; }
        public double Y { set; get; }
        public double Z { set; get; }
        public string CustomInfo { set; get; }
        public AttributeList Attributes { get; }
        
        public Entity(string entityName, int entityId, ItemType type, int entityOwnerID, int entityLocationID, int entityFlag, bool entityContraband, bool entitySingleton, int entityQuantity, double entityX, double entityY, double entityZ, string entityCustomInfo, AttributeList attributes, ItemFactory itemFactory)
        {
            this.Name = entityName;
            this.ID = entityId;
            this.Type = type;
            this.OwnerID = entityOwnerID;
            this.LocationID = entityLocationID;
            this.Flag = entityFlag;
            this.Contraband = entityContraband;
            this.Singleton = entitySingleton;
            this.Quantity = entityQuantity;
            this.X = entityX;
            this.Y = entityY;
            this.Z = entityZ;
            this.CustomInfo = entityCustomInfo;
            this.Attributes = attributes;

            this.mItemFactory = itemFactory;
        }
    }
}
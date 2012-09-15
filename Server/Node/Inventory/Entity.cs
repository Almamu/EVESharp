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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EVESharp.Database;

namespace EVESharp.Inventory
{
    public class Entity
    {
        public Entity(string entityItemName, int entityItemID, int entityTypeID, int entityOwnerID, int entityLocationID, int entityFlag, bool entityContraband, bool entitySingleton, int entityQuantity, double entityX, double entityY, double entityZ, string entityCustomInfo)
        {
            itemName = entityItemName;
            itemID = entityItemID;
            typeID = entityTypeID;
            ownerID = entityOwnerID;
            locationID = entityLocationID;
            flag = entityFlag;
            contraband = entityContraband;
            singleton = entitySingleton;
            quantity = entityQuantity;
            x = entityX;
            y = entityY;
            Z = entityZ;
            customInfo = entityCustomInfo;
        }

        public void SetSingleton(bool newSingleton, bool notify)
        {
            if (notify)
            {
                // Notify the client
            }

            if (newSingleton == true)
            {
                /*
                 * We need to add a new item to the database cloning most of the info here
                 * then add the new item to the local items list
                 */
                if (quantity > 1)
                {
                    int newQuantity = quantity--;
                    ItemFactory.GetItemManager().CreateItem(itemName, typeID, ownerID, locationID, flag, contraband, false, newQuantity, x, y, Z, customInfo);
                }
            }

            singleton = newSingleton;
        }

        public void SetItemName(string newItemName, bool notify)
        {
            if (notify)
            {
                // Notify the ownerID
            }

            itemName = newItemName;
            ItemDB.SetItemName(itemID, itemName);
        }

        public void MoveItem(int newLocationID, bool notify)
        {
            if (notify)
            {
                // Notify the ownerID
            }

            locationID = newLocationID;

            ItemDB.SetLocation(itemID, locationID);
        }

        public void TransferOwnership(int newOwnerID, bool notify)
        {
            if (notify)
            {
                // Notify the old ownerID and the new ownerID
            }

            ownerID = newOwnerID;
            ItemDB.SetOwner(itemID, ownerID);
        }

        public void SetFlag(int newFlag, bool notify)
        {
            if (notify)
            {
                // Notify the client
            }

            flag = newFlag;
            ItemDB.SetItemFlag(itemID, flag);
        }

        public void ChangeCustomInfo(string newCustomInfo)
        {
            customInfo = newCustomInfo;
            ItemDB.SetCustomInfo(itemID, customInfo);
        }

        public void SetQuantity(int newQuantity, bool notify)
        {
            if (notify)
            {
                // Notify the client
            }

            quantity = newQuantity;
            ItemDB.SetQuantity(itemID, quantity);
        }

        public void LoadAttributes()
        {
            // Load attributes
            attributes = new Attributes();

            attributes.LoadAttributes(itemID, typeID);
        }

        public string itemName {private set; get;}
        public int itemID { private set; get; }
        public int typeID { private set; get; }
        public int ownerID { private set; get; }
        public int locationID { private set; get; }
        public int flag { private set; get; }
        public bool contraband { private set; get; }
        public bool singleton { private set; get; }
        public int quantity { private set; get; }
        public double x { private set; get; }
        public double y { private set; get; }
        public double Z { private set; get; }
        public string customInfo { private set; get; }
        public Attributes attributes { private set; get; }
    }
}

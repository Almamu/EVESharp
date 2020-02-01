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
using Node.Database;

namespace Node.Inventory
{
    public class Blueprint : Entity
    {
        public Blueprint(string entityItemName, int entityItemID, int entityTypeID, int entityOwnerID, int entityLocationID, int entityFlag, bool entityContraband, bool entitySingleton, int entityQuantity, double entityX, double entityY, double entityZ, string entityCustomInfo, ItemDB itemDB, ItemFactory itemFactory)
            : base(entityItemName, entityItemID, entityTypeID, entityOwnerID, entityLocationID, entityFlag, entityContraband, entitySingleton, entityQuantity, entityX, entityY, entityZ, entityCustomInfo, itemDB, itemFactory)
        {
            
        }

        public Blueprint(Entity from) : base(from.itemName, from.itemID, from.typeID, from.ownerID, from.locationID, from.flag, from.contraband, from.singleton, from.quantity, from.x, from.y, from.Z, from.customInfo, from.mItemDB, from.mItemFactory)
        {

        }

        public void SetBlueprintInfo(bool newCopy, int newMaterialLevel, int newProductivityLevel, int newLicensedProductionRunsRemainig, bool sqlUpdate)
        {
            copy = newCopy;
            materialLevel = newMaterialLevel;
            productivityLevel = newProductivityLevel;
            licensedProductionRunsRemaining = newLicensedProductionRunsRemainig;

            if (sqlUpdate)
            {
                this.mItemDB.SetBlueprintInfo(itemID, copy, materialLevel, productivityLevel, licensedProductionRunsRemaining);
            }
        }

        public bool copy { private set; get; }
        public int materialLevel { private set; get; }
        public int productivityLevel { private set; get; }
        public int licensedProductionRunsRemaining { private set; get; }
    }
}

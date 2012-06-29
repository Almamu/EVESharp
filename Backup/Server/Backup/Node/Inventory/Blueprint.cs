using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EVESharp.Database;

namespace EVESharp.Inventory
{
    public class Blueprint : Entity
    {
        public Blueprint(string entityItemName, int entityItemID, int entityTypeID, int entityOwnerID, int entityLocationID, int entityFlag, bool entityContraband, bool entitySingleton, int entityQuantity, double entityX, double entityY, double entityZ, string entityCustomInfo)
            : base(entityItemName, entityItemID, entityTypeID, entityOwnerID, entityLocationID, entityFlag, entityContraband, entitySingleton, entityQuantity, entityX, entityY, entityZ, entityCustomInfo)
        {
            
        }

        public Blueprint(Entity from) : base(from.itemName, from.itemID, from.typeID, from.ownerID, from.locationID, from.flag, from.contraband, from.singleton, from.quantity, from.x, from.y, from.Z, from.customInfo)
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
                ItemDB.SetBlueprintInfo(itemID, copy, materialLevel, productivityLevel, licensedProductionRunsRemaining);
            }
        }

        public bool copy { private set; get; }
        public int materialLevel { private set; get; }
        public int productivityLevel { private set; get; }
        public int licensedProductionRunsRemaining { private set; get; }
    }
}

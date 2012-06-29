using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Common;
using Marshal;
using System.Threading;

using EVESharp.Database;
using EVESharp.Inventory;
using EVESharp.Inventory.SystemEntities;

namespace EVESharp
{
    public static class SystemManager
    {
        static private List<SolarSystem> solarSystemsLoaded = new List<SolarSystem>(); // Should be changed to SolarSystem class when available
        public static bool LoadSolarSystems(PyList solarSystems)
        {
            // We should not load any solar system
            if (solarSystems.Items[0] is PyNone)
            {
                return true;
            }

            // First of all check for loaded systems and unload the ones that are not needed
            foreach (SolarSystem solarSystem in solarSystemsLoaded)
            {
                bool found = false;

                // Loop the PyList to see if it should still be loaded
                foreach (PyInt listID in solarSystems.Items)
                {
                    if (listID.Value == solarSystem.itemID)
                    {
                        found = true;
                        break;
                    }
                }

                if (found == false)
                {
                    // Unload the solar system
                    if (UnloadSolarSystem(solarSystem.itemID) == false)
                    {
                        return false;
                    }
                }
            }

            // Now iterate the PyList and load the new solarSystems
            foreach (PyInt listID in solarSystems.Items)
            {
                foreach (SolarSystem solarSystem in solarSystemsLoaded)
                {
                    if (solarSystem.itemID == listID.Value)
                    {
                        if (LoadSolarSystem(solarSystem) == false)
                        {
                            return false;
                        }
                    }
                }
            }

            return true;
        }

        private static bool BootupSolarSystem(int solarSystemID)
        {
            List<Entity> items = ItemDB.GetItemsLocatedAt(solarSystemID);

            if (items == null)
            {
                return false;
            }

            // Add the items to the ItemFactory
            ItemFactory.GetItemManager().LoadInventory(solarSystemID);

            return true;
        }

        public static bool UnloadSolarSystem(int solarSystemID)
        {
            // We should do the unload work here
            
            // Update the database
            GeneralDB.UnloadSolarSystem(solarSystemID);

            return true;
        }

        public static bool LoadSolarSystem(SolarSystem solarSystem)
        {
            // We should do the load work here
            BootupSolarSystem(solarSystem.itemID);

            // Update the database
            GeneralDB.LoadSolarSystem(solarSystem.itemID);

            // Update the list
            solarSystemsLoaded.Add(solarSystem);

            // Update the ItemManager
            ItemFactory.GetItemManager().LoadItem(solarSystem.itemID);

            return true;
        }

        public static bool LoadUnloadedSolarSystems()
        {
            // Get all the solarSystems not loaded and load them
            List<int> solarSystems = GeneralDB.GetUnloadedSolarSystems();

            // Load the not-loaded solar systems
            foreach (int solarSystemID in solarSystems)
            {
                // We can assume we dont have it in the list, as we've queryed for the non-loaded solarSystems
                SolarSystem solarSystem = new SolarSystem(ItemDB.LoadItem(solarSystemID), ItemDB.GetSolarSystemInfo(solarSystemID)); // Create the solarSystem class
                
                if (LoadSolarSystem(solarSystem) == false)
                {
                    return false;
                }

            }
            return true;
        }
    }
}

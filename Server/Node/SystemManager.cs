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
using System.Runtime.CompilerServices;
using System.Text;
using Common;
using PythonTypes;
using System.Threading;
using Common.Database;
using Node.Database;
using Node.Inventory;
using Node.Inventory.SystemEntities;
using PythonTypes.Types.Primitives;

namespace Node
{
    public class SystemManager
    {
        private DatabaseConnection mDatabase = null;
        private GeneralDB mGeneralDB = null;
        private ItemDB mItemDB = null;
        private ItemFactory mItemFactory = null;
        
        private List<SolarSystem> solarSystemsLoaded = new List<SolarSystem>();
        public bool LoadSolarSystems(PyList solarSystems)
        {
            // We should not load any solar system
            if (solarSystems[0] is PyNone)
            {
                return true;
            }

            // First of all check for loaded systems and unload the ones that are not needed
            foreach (SolarSystem solarSystem in solarSystemsLoaded)
            {
                bool found = false;

                // Loop the PyList to see if it should still be loaded
                foreach (PyInteger listID in solarSystems)
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
            foreach (PyInteger listID in solarSystems)
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

        private bool BootupSolarSystem(int solarSystemID)
        {
            List<Entity> items = this.mItemDB.GetItemsLocatedAt(solarSystemID);

            if (items == null)
            {
                return false;
            }

            // Add the items to the ItemFactory
            this.mItemFactory.ItemManager.LoadInventory(solarSystemID);

            return true;
        }

        public bool UnloadSolarSystem(int solarSystemID)
        {
            // We should do the unload work here
            
            // Update the database
            this.mGeneralDB.UnloadSolarSystem(solarSystemID);

            return true;
        }

        public bool LoadSolarSystem(SolarSystem solarSystem)
        {
            // We should do the load work here
            BootupSolarSystem(solarSystem.itemID);

            // Update the database
            this.mGeneralDB.LoadSolarSystem(solarSystem.itemID);

            // Update the list
            solarSystemsLoaded.Add(solarSystem);

            // Update the ItemManager
            this.mItemFactory.ItemManager.LoadItem(solarSystem.itemID);

            return true;
        }

        public bool LoadUnloadedSolarSystems()
        {
            // Get all the solarSystems not loaded and load them
            List<int> solarSystems = this.mGeneralDB.GetUnloadedSolarSystems();

            // Load the not-loaded solar systems
            foreach (int solarSystemID in solarSystems)
            {
                // We can assume we dont have it in the list, as we've queryed for the non-loaded solarSystems
                SolarSystem solarSystem = new SolarSystem(this.mItemDB.LoadItem(solarSystemID), this.mItemDB.GetSolarSystemInfo(solarSystemID)); // Create the solarSystem class
                
                if (LoadSolarSystem(solarSystem) == false)
                {
                    return false;
                }
            }
            
            return true;
        }

        public SystemManager(DatabaseConnection db, ItemFactory itemFactory)
        {
            this.mDatabase = db;
            this.mGeneralDB = new GeneralDB(this.mDatabase);
            this.mItemDB = new ItemDB(this.mDatabase);
            this.mItemFactory = itemFactory;
        }
    }
}

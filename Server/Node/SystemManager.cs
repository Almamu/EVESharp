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

using System.Collections.Generic;
using Common.Database;
using Node.Database;
using Node.Inventory;
using Node.Inventory.Items;
using Node.Inventory.SystemEntities;
using PythonTypes.Types.Primitives;

namespace Node
{
    public class SystemManager
    {
        private readonly DatabaseConnection mDatabase = null;
        private readonly GeneralDB mGeneralDB = null;
        private readonly ItemFactory mItemFactory = null;

        private readonly List<SolarSystem> mLoadedSolarSystems = new List<SolarSystem>();

        public bool LoadSolarSystems(PyList solarSystems)
        {
            // We should not load any solar system
            if (solarSystems[0] is PyNone)
                return true;

            // First of all check for loaded systems and unload the ones that are not needed
            foreach (SolarSystem solarSystem in mLoadedSolarSystems)
            {
                bool found = false;

                // Loop the PyList to see if it should still be loaded
                foreach (PyInteger listID in solarSystems)
                {
                    if (listID.Value == solarSystem.ID)
                    {
                        found = true;
                        break;
                    }
                }

                if (found == false)
                {
                    // Unload the solar system
                    if (UnloadSolarSystem(solarSystem) == false)
                        return false;
                }
            }

            // Now iterate the PyList and load the new solarSystems
            foreach (PyInteger listID in solarSystems)
            {
                bool found = false;
                
                foreach (SolarSystem solarSystem in mLoadedSolarSystems)
                {
                    if (solarSystem.ID == listID.Value)
                    {
                        found = true;
                        break;
                    }
                }

                if (found == false)
                {
                    if (LoadSolarSystem(listID) == false)
                        return false;
                }
            }

            return true;
        }

        public bool UnloadSolarSystem(SolarSystem solarSystem)
        {
            // remove references off the ItemFactory
            this.mItemFactory.ItemManager.UnloadItem(solarSystem);
            
            // Update the database
            this.mGeneralDB.MarkSolarSystemAsUnloaded(solarSystem);

            return true;
        }

        public bool LoadSolarSystem(int solarSystemID)
        {
            // load the item into memory
            SolarSystem solarSystem = this.mItemFactory.ItemManager.LoadItem(solarSystemID) as SolarSystem;
            
            // ensure the items in the solar system are loaded

            // Update the database
            this.mGeneralDB.MarkSolarSystemAsLoaded(solarSystem);

            // Update the list
            this.mLoadedSolarSystems.Add(solarSystem);

            return true;
        }

        public bool LoadUnloadedSolarSystems()
        {
            // Get all the solarSystems not loaded and load them
            List<int> solarSystems = this.mGeneralDB.GetUnloadedSolarSystems();

            // Load the not-loaded solar systems
            foreach (int solarSystemID in solarSystems)
            {
                if (LoadSolarSystem(solarSystemID) == false)
                    return false;
            }

            return true;
        }

        public SystemManager(DatabaseConnection db, ItemFactory itemFactory)
        {
            this.mDatabase = db;
            this.mGeneralDB = new GeneralDB(this.mDatabase);
            this.mItemFactory = itemFactory;
        }
    }
}
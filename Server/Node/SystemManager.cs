/*
    ------------------------------------------------------------------------------------
    LICENSE:
    ------------------------------------------------------------------------------------
    This file is part of EVE#: The EVE Online Server Emulator
    Copyright 2021 - EVE# Team
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
using Node.Database;
using Node.Inventory;
using Node.Inventory.SystemEntities;
using PythonTypes.Types.Primitives;

namespace Node
{
    public class SystemManager
    {
        private GeneralDB GeneralDB { get; }
        private ItemFactory ItemFactory { get; }

        private readonly Dictionary<int, SolarSystem> mLoadedSolarSystems = new Dictionary<int, SolarSystem>();

        public void LoadSolarSystems(PyList solarSystems)
        {
            if (solarSystems.Count == 0)
                return;

            foreach (PyDataType solarSystem in solarSystems)
            {
                if (solarSystem is PyInteger == false)
                    continue;

                int solarSystemID = solarSystem as PyInteger;

                if (this.mLoadedSolarSystems.ContainsKey(solarSystemID) == true)
                    continue;

                this.LoadSolarSystem(solarSystemID);
            }
        }

        public void UnloadSolarSystem(SolarSystem solarSystem)
        {
            solarSystem.BelongsToUs = false;
        }

        public void LoadSolarSystem(int solarSystemID)
        {
            SolarSystem solarSystem = this.ItemFactory.ItemManager.GetSolarSystem(solarSystemID);

            solarSystem.BelongsToUs = true;
            
            // Update the list
            this.mLoadedSolarSystems.Add(solarSystemID, solarSystem);
        }

        public SystemManager(GeneralDB generalDB, ItemFactory itemFactory)
        {
            this.GeneralDB = generalDB;
            this.ItemFactory = itemFactory;
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Common;
using Marshal;
using System.Threading;
using EVESharp.Database;

namespace EVESharp
{
    public static class SystemManager
    {
        static private List<int> solarSystemsLoaded = new List<int>(); // Should be changed to SolarSystem class when available
        public static bool LoadSolarSystems(PyList solarSystems)
        {
            // If the first item is a none, load all the possible solarSystems
            if (solarSystems.Items[0] is PyNone)
            {
                return LoadUnloadedSolarSystems();
            }

            // First of all check for loaded systems and unload the ones that are not needed
            foreach (int solarSystemID in solarSystemsLoaded)
            {
                bool found = false;

                // Loop the PyList to see if it should still be loaded
                foreach (PyInt listID in solarSystems.Items)
                {
                    if (listID.Value == solarSystemID)
                    {
                        found = true;
                        break;
                    }
                }

                if (found == false)
                {
                    // Unload the solar system
                    if (UnloadSolarSystem(solarSystemID) == false)
                    {
                        return false;
                    }
                }
            }

            // Now iterate the PyList and load the new solarSystems
            foreach (PyInt listID in solarSystems.Items)
            {
                bool loaded = false;

                foreach (int solarSystemID in solarSystemsLoaded)
                {
                    if (solarSystemID == listID.Value)
                    {
                        loaded = true;
                    }
                }

                if (loaded == false)
                {
                    if (LoadSolarSystem(listID.Value) == false)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        public static bool UnloadSolarSystem(int solarSystemID)
        {
            // We should do the unload work here
            return true;
        }

        public static bool LoadSolarSystem(int solarSystemID)
        {
            // We should do the load work here
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
                if (LoadSolarSystem(solarSystemID) == false)
                {
                    return false;
                }

            }
            return true;
        }
    }
}

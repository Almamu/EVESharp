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
using MySql.Data.MySqlClient;

namespace EVESharp.ClusterControler.Database
{
    public static class GeneralDB
    {
        public static void ResetSolarSystemStatus()
        {
            foreach (var item in Database.context.solarsystemsloadeds)
            {
                item.nodeID = 0;
            }
        }

        public static void ResetSolarSystemStatus(int solarSystemID)
        {
            var systems = from h in Database.context.solarsystemsloadeds where h.solarSystemID == solarSystemID select h;

            foreach (var system in systems)
            {
                system.nodeID = solarSystemID;
            }
        }

        public static void ResetItemsStatus()
        {
            foreach (var item in Database.context.entities)
            {
                item.nodeID = 0;
            }
        }
    }
}

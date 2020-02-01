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

using Common;
using Common.Database;

namespace ClusterControler.Database
{
    public class GeneralDB : DatabaseAccessor
    {
        public void ResetSolarSystemStatus()
        {
            if (Database.Query("UPDATE solarSystemsLoaded SET nodeID = 0") == false)
            {
                Log.Error("GeneralDB", "Cannot reset solar systems nodeID to 0");
            }
        }

        public void ResetSolarSystemStatus(int solarSystemID)
        {
            if (Database.Query("UPDATE solarSystemsLoaded SET nodeID = 0 WHERE solarSystemID = " + solarSystemID.ToString()) == false)
            {
                Log.Error("GeneralDB", "Cannot reset solar system nodeID to 0 for solar system " + solarSystemID.ToString());
            }
        }

        public void ResetItemsStatus()
        {
            if (Database.Query("UPDATE entity SET nodeID = 0") == false)
            {
                Log.Error("GeneralDB", "Cannot reset nodeID for items");
            }
        }

        public GeneralDB(Common.Database.DatabaseConnection db) : base(db)
        {
        }
    }
}

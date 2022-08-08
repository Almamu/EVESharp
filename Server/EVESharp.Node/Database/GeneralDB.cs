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

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using EVESharp.EVE.Data;
using EVESharp.EVE.Data.Configuration;
using EVESharp.PythonTypes.Types.Collections;
using EVESharp.PythonTypes.Types.Database;
using EVESharp.PythonTypes.Types.Primitives;

namespace EVESharp.Node.Database;

public class GeneralDB
{
    private IDatabaseConnection Database { get; }

    public GeneralDB (IDatabaseConnection database)
    {
        Database = database;
    }

    public long GetNodeWhereSolarSystemIsLoaded (int solarSystemID)
    {
        IDbConnection connection = null;
        DbDataReader reader = Database.Select (
            ref connection,
            "SELECT nodeID FROM invItems WHERE itemID = @solarSystemID",
            new Dictionary <string, object> {{"@solarSystemID", solarSystemID}}
        );

        using (connection)
        using (reader)
        {
            if (reader.Read () == false)
                return 0;

            return reader.GetInt64 (0);
        }
    }

    public void UpdateCharacterLogoffDateTime (int characterID)
    {
        Database.Prepare (
            "UPDATE chrInformation SET logoffDateTime = @date, online = 0 WHERE characterID = @characterID",
            new Dictionary <string, object>
            {
                {"@characterID", characterID},
                {"@date", DateTime.UtcNow.ToFileTimeUtc ()}
            }
        );
    }

    public void ResetCharacterOnlineStatus ()
    {
        Database.Query ("UPDATE chrInformation SET online = 0");
    }
}
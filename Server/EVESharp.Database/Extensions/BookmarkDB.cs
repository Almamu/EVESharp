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
using EVESharp.Database.Types;
using EVESharp.Types;
using EVESharp.Types.Collections;

namespace EVESharp.Database.Extensions;

public static class BookmarkDB
{
    public static Rowset ChrBookmarksGet (this IDatabase Database, int ownerID)
    {
        return Database.Rowset (
            "ChrBookmarksGet",
            new Dictionary <string, object> {{"_ownerID", ownerID}}
        );
    }

    public static void ChrBookmarksDelete (this IDatabase Database, int ownerID, PyList <PyInteger> bookmarkIDs)
    {
        Database.QueryProcedure (
            "ChrBookmarksDelete",
            new Dictionary <string, object>
            {
                {"_ownerID", ownerID},
                {"_bookmarkIDs", PyString.Join (',', bookmarkIDs).Value}
            }
        );
    }

    public static ulong ChrBookmarksCreate (
        this IDatabase Database, int    ownerID, int itemID, int typeID, string memo, string comment, double x,
        double                   y,        double z,       int locationID
    )
    {
        return Database.Scalar <ulong> (
            "ChrBookmarksCreate",
            new Dictionary <string, object>
            {
                {"_ownerID", ownerID},
                {"_itemID", itemID},
                {"_typeID", typeID},
                {"_memo", memo},
                {"_comment", comment},
                {"_date", DateTime.UtcNow.ToFileTimeUtc ()},
                {"_x", x},
                {"_y", y},
                {"_z", z},
                {"_locationID", locationID}
            }
        );
    }
}
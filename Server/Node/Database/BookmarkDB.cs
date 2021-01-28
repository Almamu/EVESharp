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
using Common.Database;
using PythonTypes.Types.Database;

namespace Node.Database
{
    public class BookmarkDB : DatabaseAccessor
    {
        public BookmarkDB(DatabaseConnection db) : base(db)
        {
        }

        public Rowset GetBookmarks(int ownerID)
        {
            return Database.PrepareRowsetQuery(
	            "SELECT bookmarkID, ownerID, itemID, typeID, memo, comment, created, x, y, z, locationID FROM chrBookmarks WHERE ownerID = @ownerID",
	            new Dictionary<string, object>()
	            {
		            {"@ownerID", ownerID}
	            }
	        );
        }

        public ulong CreateBookmark(int characterID, int itemID, int typeID, string memo, string comment, double x, double y,
            double z, int locationID)
        {
            return Database.PrepareQueryLID(
                "INSERT INTO chrBookmarks(ownerID, itemID, typeID, memo, comment, created, x, y, z, locationID)VALUES(@characterID, @itemID, @typeID, @memo, @comment, @date, @x, @y, @z, @locationID)",
                new Dictionary<string, object>()
                {
                    {"@characterID", characterID},
                    {"@itemID", itemID},
                    {"@typeID", typeID},
                    {"@memo", memo},
                    {"@comment", comment},
                    {"@date", DateTime.UtcNow.ToFileTimeUtc ()},
                    {"@x", x},
                    {"@y", y},
                    {"@z", z},
                    {"@locationID", locationID}
                }
            );
        }

        public void DeleteBookmark(List<int> bookmarkIDs, int ownerID)
        {
            // do not remove anything if the count is not greater than 0
            if (bookmarkIDs.Count == 0)
                return;

            Database.PrepareQuery(
                "DELETE FROM chrBookmarks WHERE ownerID = @characterID AND bookmarkID IN(" + String.Join(',', bookmarkIDs) + ")",
                new Dictionary<string, object>()
                {
                    {"@characterID", ownerID}
                }
            );
        }
    }
}
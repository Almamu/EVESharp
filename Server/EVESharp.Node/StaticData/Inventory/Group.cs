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

namespace EVESharp.Node.StaticData.Inventory
{
    public class Group
    {
        public int ID { get; }
        public Category Category { get; }
        public string Name { get; }
        public string Description { get; }
        public int GraphicID { get; }
        public bool UseBasePrice { get; }
        public bool AllowManufacture { get; }
        public bool AllowRecycler { get; }
        public bool Anchored { get; }
        public bool Anchorable { get; }
        public bool FittableNonSingleton { get; }
        public bool Published { get; }

        public Group(int id, Category category, string name, string description, int graphicId, bool useBasePrice, bool allowManufacture, bool allowRecycler, bool anchored, bool anchorable, bool fittableNonSingleton, bool published)
        {
            this.ID = id;
            this.Category = category;
            this.Name = name;
            this.Description = description;
            this.GraphicID = graphicId;
            this.UseBasePrice = useBasePrice;
            this.AllowManufacture = allowManufacture;
            this.AllowRecycler = allowRecycler;
            this.Anchored = anchored;
            this.Anchorable = anchorable;
            this.FittableNonSingleton = fittableNonSingleton;
            this.Published = published;
        }
    }
}
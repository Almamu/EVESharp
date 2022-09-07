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
using EVESharp.Database.Inventory;
using EVESharp.EVE.Data.Inventory;
using EVESharp.PythonTypes.Database;
using EVESharp.PythonTypes.Types.Database;

namespace EVESharp.Node.Data.Inventory;

public class Categories : ICategories
{
    private readonly Dictionary <int, Category> mCategories;

    public Category this [int id] => this.mCategories [id];

    public Categories (IDatabaseConnection Database)
    {
        this.mCategories = Database.InvLoadItemCategories ();
    }
}
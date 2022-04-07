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
using EVESharp.Node.Database;
using EVESharp.Node.StaticData.Inventory;
using AttributeInfo = EVESharp.Node.Inventory.Items.Attributes.Attribute;

namespace EVESharp.Node.Inventory;

public class AttributeManager
{
    private Dictionary <int, Attribute> mAttributes;
    private ItemDB                      ItemDB { get; }

    public Dictionary <int, Dictionary <int, AttributeInfo>> DefaultAttributes { get; private set; }

    public Attribute this [int        id] => this.mAttributes [id];
    public Attribute this [Attributes id] => this [(int) id];

    public AttributeManager (ItemDB itemDB)
    {
        ItemDB = itemDB;
    }

    public void Load ()
    {
        this.mAttributes  = ItemDB.LoadAttributesInformation ();
        DefaultAttributes = ItemDB.LoadDefaultAttributes ();
    }
}
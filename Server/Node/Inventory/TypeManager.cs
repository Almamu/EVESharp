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

using System.Collections;
using System.Collections.Generic;
using Node.Database;
using Node.Dogma;
using Node.Inventory.Items;
using Node.StaticData;
using Node.StaticData.Inventory;

namespace Node.Inventory
{
    public class TypeManager : IReadOnlyDictionary<int, Type>
    {
        private ItemDB ItemDB { get; }
        private Dictionary<int, Type> mTypes = null;
        public ExpressionManager ExpressionManager { get; }

        public void Load()
        {
            this.mTypes = this.ItemDB.LoadItemTypes(this.ExpressionManager);
        }

        public bool ContainsKey(int typeID)
        {
            return this.mTypes.ContainsKey(typeID);
        }

        public bool TryGetValue(int typeID, out Type value)
        {
            return this.mTypes.TryGetValue(typeID, out value);
        }

        public Type this[int id] => this.mTypes[id];
        public Type this[ItemTypes id] => this[(int) id];
        public IEnumerable<int> Keys  => this.mTypes.Keys;
        public IEnumerable<Type> Values => this.mTypes.Values;
        public int Count => this.mTypes.Count;

        public TypeManager(ItemDB itemDB, ExpressionManager expressionManager)
        {
            this.ItemDB = itemDB;
            this.ExpressionManager = expressionManager;
        }

        public IEnumerator<KeyValuePair<int, Type>> GetEnumerator()
        {
            return this.mTypes.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}